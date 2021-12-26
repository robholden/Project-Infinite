import { Component, ElementRef, Injector, Input, OnInit, ViewChild } from '@angular/core';

import { CustomEvent } from '@shared/enums';
import { deepCopy, enumValues, inputWatcher, parseEnum, uniqueArray, wait } from '@shared/functions';
import { Boundry, Country, CustomError, Location, PageRequest, Picture, PictureOrderBy, PictureSearch, SMap } from '@shared/models';
import { EventService } from '@shared/services';
import { FiltersService, PictureService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

import { InfiniteComponent } from '@app/features/infinite-component.helper';
import { fade } from '@app/functions/animations.fn';
import { boundryToBounds, LeafletMapOptions, pictureMarkers } from '@app/functions/leaflet.fn';
import { PictureEditModal } from '@app/modals/picture-edit/picture-edit.modal';
import { ModalController } from '@app/shared/controllers/modal';

interface FilterLocation extends Country {
    locations: Location[];
}

interface Filter {
    countries: SMap<boolean>;
    locations: SMap<boolean>;
    tags: SMap<boolean>;
    bounds: Boundry;
}

interface Filters {
    locations: FilterLocation[];
    tags: string[];
}

@Component({
    selector: 'sc-show-pictures',
    templateUrl: './show-pictures.component.html',
    styleUrls: ['./show-pictures.component.scss'],
    animations: [fade],
})
export class ShowPicturesComponent extends InfiniteComponent<Picture, PictureSearch> implements OnInit {
    enum_values = enumValues(PictureOrderBy, PictureOrderBy.None);

    @Input() showFilters: boolean;
    @ViewChild('locationInput', { static: false }) locationInput: ElementRef;
    @ViewChild('tagInput', { static: false }) tagInput: ElementRef;

    allFilters: Filters;
    filters: Filters;
    filter: Filter = { countries: {}, locations: {}, tags: {}, bounds: null };

    countryMap: SMap<string> = {};

    mapOptions: LeafletMapOptions;

    constructor(
        service: PictureService,
        events: EventService,
        public authState: AuthState,
        private injector: Injector,
        private modalCtrl: ModalController,
        private filtersService: FiltersService
    ) {
        super(
            injector,
            service,
            {
                pager: new PageRequest(),
                def: {},
                filterParams: [],
                queryConfig: { countries: 'array', locations: 'array', orderBy: (value: string) => parseEnum(value, PictureOrderBy) },
                onResult: async () => {
                    this.mapOptions = null;
                    await wait(0);

                    const mapOptions = pictureMarkers(this.injector, this.result.rows);
                    if (this.params.bounds) mapOptions.fitBounds = boundryToBounds(this.params.bounds);

                    this.mapOptions = mapOptions;
                },
            },
            { defer: () => this.loadFilters() }
        );

        events.register(
            CustomEvent.Uploaded,
            async () => {
                if (!this.params.drafts) return;
                await this.reload();
            },
            'show_pictures'
        );
    }

    ngOnInit(): void {}

    setFilters() {
        const countries = Object.keys(this.filter.countries).filter((code) => this.filter.countries[code]);
        const locations = Object.keys(this.filter.locations).filter((name) => {
            const country = this.filters.locations.find((c) => c.locations.some((l) => l.name === name));
            return this.filter.locations[name] && (!country || !countries.some((c) => c === country.code));
        });
        const tags = Object.keys(this.filter.tags).filter((value) => this.filter.tags[value]);

        this.params.countries = countries.length > 0 ? countries : null;
        this.params.locations = locations.length > 0 ? locations : null;
        this.params.tags = tags.length > 0 ? tags : null;

        this.reload();
    }

    async edit(index: number) {
        const modal = this.modalCtrl.add('edit-picture', PictureEditModal, { pictureId: this.result.rows[index].pictureId });
        const updated = await modal.present();

        if (updated) this.result.rows[index].name = updated.name;
        else if (updated === false) this.result.rows.splice(index, 1);
    }

    private async loadFilters() {
        if (!this.showFilters) return;

        const filters = await this.filtersService.get();
        if (filters instanceof CustomError) return;

        // Combine locations into countries
        const countries = uniqueArray(
            filters.locations.map((l) => l.country),
            (l1, l2) => l1.name === l2.name
        );
        const locations = countries.map((country) => ({ ...country, locations: filters.locations.filter((l) => l.country.name === country.name) }));

        // Create country code map lookup for UI
        this.countryMap = countries.reduce((acc, curr: Country) => {
            acc[curr.code] = curr.name;
            return acc;
        }, {} as SMap<string>);

        this.allFilters = { locations, tags: filters.tags.map((t) => t.value) };
        this.filters = deepCopy(this.allFilters);

        // Make sure we only show valid filter values
        this.filter = {
            countries: (this.params.countries || [])
                .filter((code) => locations.some((loc) => loc.code === code))
                .reduce((acc, country) => {
                    acc[country] = true;
                    return acc;
                }, {}),
            locations: (this.params.locations || [])
                .filter((name) => locations.some((country) => country.locations.some((loc) => loc.name === name)))
                .reduce((acc, location) => {
                    acc[location] = true;
                    return acc;
                }, {}),
            tags: (this.params.tags || [])
                .filter((value) => filters.tags.some((tag) => tag.value === value))
                .reduce((acc, value) => {
                    acc[value] = true;
                    return acc;
                }, {}),
            bounds: filters.bounds,
        };
        this.setFilters();

        await wait(0);

        this.watchLocationFilter();
        this.watchTagFilter();
    }

    private watchLocationFilter() {
        inputWatcher(this.locationInput, (value) => {
            value = (value || '').toLowerCase();

            if (!value) {
                this.filters.locations = this.allFilters.locations;
                return;
            }

            this.filters.locations = deepCopy(this.allFilters).locations.reduce((list, curr) => {
                // Filter locations
                curr.locations = curr.locations.filter((l) => l.name.toLowerCase().includes(value));

                // Check country name
                if (curr.name.toLowerCase().includes(value) || curr.locations.length > 0) {
                    list.push(curr);
                }

                return list;
            }, []);
        });
    }

    private watchTagFilter() {
        inputWatcher(this.tagInput, (value) => {
            value = (value || '').toLowerCase();

            if (!value) {
                this.filters.tags = this.allFilters.tags;
                return;
            }

            this.filters.tags = this.allFilters.tags.filter((t) => t.toLowerCase().includes(value));
        });
    }
}
