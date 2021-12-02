import { ApplicationRef, ComponentFactoryResolver, EmbeddedViewRef, Injector } from '@angular/core';

import { PicturePopupComponent } from '@app/components/picture-popup/picture-popup.component';

import { environment } from '@env/environment';

import { getPictureCoords, Picture } from '@shared/models';

import { FeatureGroup, featureGroup, icon, latLng, LatLngBounds, Layer, MapOptions, marker, tileLayer } from 'leaflet';

export interface MapData {
    bounds: LatLngBounds;
    options: MapOptions;
}

export function getTileLayer() {
    return tileLayer(`https://{s}-tiles.locationiq.com/v3/streets/r/{z}/{x}/{y}.png?key=${environment.locationIQ.token}`, {
        maxZoom: 18,
        attribution: '...',
    });
}

export function pictureMarkers(injector: Injector, pictures: Picture[], source?: Picture): MapData {
    if ((pictures || []).length === 0 && !source) return null;

    const fac = injector.get(ComponentFactoryResolver);
    const appRef = injector.get(ApplicationRef);

    let sourceMarkerGroup: FeatureGroup<any>;
    const sourceCoords = getPictureCoords(source);
    if (sourceCoords) {
        sourceMarkerGroup = featureGroup([
            marker(latLng(sourceCoords.lat, sourceCoords.lng), {
                autoPan: true,
                icon: icon({
                    iconSize: [20, 17],
                    iconUrl: 'assets/images/map-pin-regular.svg',
                    iconRetinaUrl: 'assets/images/map-pin-regular.svg',
                    shadowUrl: null,
                }),
            }),
        ]);
    }

    const coords = (pictures || [])
        .map((picture) => {
            const coord = getPictureCoords(picture);
            return coord ? { picture, coord } : null;
        })
        .filter((p) => !!p);

    if (!sourceMarkerGroup && coords.length === 0) {
        return null;
    }

    let markersGroup: FeatureGroup<any>;

    if (coords.length > 0) {
        markersGroup = featureGroup(
            coords.map((value) => {
                const imageUrl = `${environment.gateway}/api/content/images/thumbnail?name=${value.picture.path}`;
                const markerLayer = marker(latLng(value.coord.lat, value.coord.lng), {
                    autoPan: true,
                    icon: icon({
                        iconSize: [20, 17],
                        iconUrl: imageUrl,
                        iconRetinaUrl: imageUrl,
                        shadowUrl: null,
                    }),
                });

                const componentRef = fac.resolveComponentFactory(PicturePopupComponent).create(injector);
                componentRef.instance.picture = value.picture;

                appRef.attachView(componentRef.hostView);
                const domElem = (componentRef.hostView as EmbeddedViewRef<any>).rootNodes[0] as HTMLElement;

                markerLayer.bindPopup(domElem, { className: 'popup-picture' });

                return markerLayer;
            })
        );
    }

    const layers: Layer[] = [getTileLayer()];
    if (markersGroup) layers.push(markersGroup);
    if (sourceMarkerGroup) layers.push(sourceMarkerGroup);

    return {
        bounds: sourceMarkerGroup ? sourceMarkerGroup.getBounds() : markersGroup.getBounds(),
        options: {
            layers,
            zoom: 12,
            minZoom: 3,
        } as MapOptions,
    };
}
