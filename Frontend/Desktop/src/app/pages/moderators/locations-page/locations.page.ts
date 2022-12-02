import { Component, ElementRef, Injector, OnInit, ViewChild } from '@angular/core';
import { Validators } from '@angular/forms';

import { fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';

import { enumValues, parseEnum, wait } from '@shared/functions';
import { CustomError, Location, LocationOrderBy, LocationSearch, PageRequest } from '@shared/models';
import { LocationService } from '@shared/services/content';

import { PagedComponent } from '@app/features/paged-component.helper';
import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';

@Component({
    selector: 'sc-locations',
    templateUrl: './locations.page.html',
    styleUrls: ['./locations.page.scss'],
})
export class LocationsPage extends PagedComponent<Location, LocationSearch> implements OnInit {
    enum_values = enumValues(LocationOrderBy, LocationOrderBy.None);

    @ViewChild('searchInput', { static: true }) searchInput: ElementRef;

    constructor(injector: Injector, private service: LocationService, private loadingCtrl: LoadingController, private alertCtrl: AlertController) {
        super(injector, service, {
            pager: new PageRequest(LocationOrderBy[LocationOrderBy.Name], 'asc'),
            def: {},
            filterParams: [],
            queryConfig: { orderBy: (value: string) => parseEnum(value, LocationOrderBy) },
        });
    }

    ngOnInit(): void {
        fromEvent(this.searchInput.nativeElement, 'keyup')
            .pipe(
                map((event: any) => event.target.value),
                debounceTime(500),
                distinctUntilChanged()
            )
            .subscribe(async (name: string) => {
                this.params.name = name;
                await this.search();

                await wait(0);
                this.searchInput.nativeElement.focus();
            });
    }

    headerFn(index: number) {
        const record = this.result.rows[index];
        const oldName = index === 0 ? null : this.result.rows[index - 1].country.name;
        const currentName = record.country.name;

        return oldName !== currentName ? currentName : null;
    }

    async loadMore() {
        await this.search({ page: this.pager.page + 1 });
    }

    async edit(index: number, field: 'name' | 'code') {
        const location = this.result.rows[index];
        const result = await this.alertCtrl.create({
            title: 'Change location ' + field,
            message: `Enter the new ${field} for <b>${location.name}</b> (${location.code}), ${location.country.name}`,
            inputs: [
                {
                    name: 'value',
                    type: 'text',
                    validators: [Validators.required],
                    placeholder: location[field],
                    value: location[field],
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Update',
                    role: 'submit',
                    colour: 'primary',
                    className: 'ml-a',
                },
            ],
        });

        if (!result || result.value === location.code) return;

        const cached = location[field];
        this.result.rows[index][field] = result.value;

        const resp = await this.loadingCtrl.addBtnWithApi(
            `${location.locationId}_${field}`,
            field === 'name' ? this.service.updateName(location.locationId, result.value) : this.service.updateCode(location.locationId, result.value)
        );
        if (resp instanceof CustomError) this.result.rows[index][field] = cached;
    }
}
