import { Coords } from './coords.model';
import { Country } from './country.model';

export interface Boundry {
    minLat: number;
    maxLat: number;
    minLng: number;
    maxLng: number;
}

export interface Location extends Coords {
    locationId: string;
    name: string;
    country: Country;
    boundry: Boundry;
}

export interface LocationSearch {
    name?: string;
}

export enum LocationOrderBy {
    None,
    Name,
}
