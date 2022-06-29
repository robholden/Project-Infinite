import { Component, EventEmitter, HostBinding, Input, OnInit, Output, SimpleChanges } from '@angular/core';

import { fieldHasChanged } from '@shared/functions';
import { Boundry } from '@shared/models';

import { boundsToBoundry, getTileLayer, LeafletMapOptions } from '@app/functions/leaflet.fn';

import * as L from 'leaflet';
import 'leaflet-area-select';

@Component({
    selector: 'sc-show-map',
    templateUrl: 'show-map.component.html',
    styleUrls: ['show-map.component.scss'],
})
export class ShowMapComponent implements OnInit {
    @Input() @HostBinding('style.height.vh') height: number = 25;
    @Input() center: number[] = [51.505, -0.09];
    @Input() fitBounds: L.LatLngBounds;
    @Input() zoom: number = 2;
    @Input() minZoom: number = 2;
    @Input() layers: L.Layer[] = [getTileLayer()];

    @Input() selectable: boolean;
    @Output() selected = new EventEmitter<Boundry>();

    @Input() options: LeafletMapOptions;

    leafletOptions: L.MapOptions | any;

    constructor() {}

    ngOnInit(): void {
        if (this.options) this.load();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (fieldHasChanged(changes, 'options')) this.load();
    }

    ready(map: L.Map) {
        let box: L.Rectangle<any>;

        map.on('areaselected', (e: any) => {
            const bounds = e.bounds as L.LatLngBounds;
            if (box) map.removeLayer(box);
            box = L.rectangle(bounds, { color: '#ff7800', weight: 1, fillOpacity: 0.05 }).addTo(map);
            map.fitBounds(bounds);
            this.selected.emit(boundsToBoundry(bounds));
        });
    }

    private load() {
        this.leafletOptions = {
            layers: this.options?.layers || this.layers,
            center: this.center,
            zoom: this.options?.zoom || this.zoom,
            minZoom: this.options?.minZoom || this.minZoom,
            selectArea: this.selectable,
        };
    }
}
