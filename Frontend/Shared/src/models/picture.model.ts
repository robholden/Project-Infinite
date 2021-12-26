import { PictureStatus } from '@shared/enums';

import { Boundry } from './';
import { Coords } from './coords.model';
import { Location } from './location.model';

export class Picture extends Coords {
    pictureId: string;
    username: string;
    name: string;
    ext: string;
    path: string;
    format: string;
    width: number;
    height: number;
    location: Location;
    concealCoords: boolean;
    coords: Coords;
    seed: string;
    createdDate: Date;
    dateTaken: Date;
    tags: string[];
    liked: boolean;
    likesTotal: number;
    status: PictureStatus;
}

export interface PictureSearch {
    countries?: string[];
    locations?: string[];
    tags?: string[];
    username?: string;
    collection?: string;
    likes?: boolean;
    drafts?: boolean;
    bounds?: Boundry;
}

export interface PictureModSearch {
    username?: string;
}

export enum PictureOrderBy {
    None,
    UploadDate,
    Name,
    DateTaken,
}

export function getPictureCoords(picture: Picture): Coords {
    if (!picture) return null;

    let lat = picture.lat;
    let lng = picture.lng;

    if (!lat || !lng) {
        if (!picture.location) {
            return null;
        }

        lat = picture.location.lat;
        lng = picture.location.lng;
    }
    return { lat, lng };
}
