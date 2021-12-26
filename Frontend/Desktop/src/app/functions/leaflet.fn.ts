import { ApplicationRef, ComponentFactoryResolver, EmbeddedViewRef, Injector } from '@angular/core';

import { environment } from '@env/environment';

import { Boundry, getPictureCoords, Picture } from '@shared/models';

import { PicturePopupComponent } from '@app/components/picture-popup/picture-popup.component';

import { FeatureGroup, featureGroup, icon, latLng, LatLngBounds, Layer, marker, tileLayer } from 'leaflet';
import * as L from 'leaflet';

export interface LeafletMapOptions {
    fitBounds?: LatLngBounds;
    layers?: L.Layer[];
    zoom?: number;
    minZoom?: number;
}

export function getTileLayer() {
    return tileLayer(`https://{s}-tiles.locationiq.com/v3/streets/r/{z}/{x}/{y}.png?key=${environment.locationIQ.token}`, {
        maxZoom: 18,
        attribution: '...',
    });
}

export function boundsToBoundry(bounds: L.LatLngBounds): Boundry {
    const topRight = bounds.getNorthEast();
    const bottomLeft = bounds.getSouthWest();

    return {
        minLat: bottomLeft.lat, // min lat / bottom-left Latitude
        maxLat: topRight.lat, // max lat / top-right Latitude
        minLng: bottomLeft.lng, // min lon / bottom-left Longitude
        maxLng: topRight.lng, // max lon / top-right Longitude
    };
}

export function boundryToBounds(boundry: Boundry): L.LatLngBounds {
    const southWest = new L.LatLng(boundry.minLat, boundry.minLng);
    const northEast = new L.LatLng(boundry.maxLat, boundry.maxLng);

    return new L.LatLngBounds(southWest, northEast);
}

export function pictureMarkers(injector: Injector, pictures: Picture[], source?: Picture): LeafletMapOptions {
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
        fitBounds: sourceMarkerGroup ? sourceMarkerGroup.getBounds() : markersGroup.getBounds(),
        layers,
        zoom: 12,
        minZoom: 3,
    };
}
