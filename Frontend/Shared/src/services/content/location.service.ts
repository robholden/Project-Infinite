import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { Boundry, CustomError, Location, LocationSearch, PagedList, PageRequest } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class LocationService {
    constructor(private api: HttpApiService) {}

    /**
     * Searches locations by given parameters
     *
     * @param route The api route to use for the endpoint
     * @param search The search criteria
     * @param pageRequest Page options
     */
    async lookup(route: string, search: LocationSearch, pageRequest: PageRequest): Promise<PagedList<Location> | CustomError> {
        return await this.api.get<PagedList<Location>>(`/content/location${route || ''}?` + obj2QueryString(search, pageRequest), {
            toastError: false,
        });
    }

    /**
     * Changes a location's name
     *
     * @param id The location id
     * @param name The new location name
     */
    async updateName(id: string, name: string): Promise<void | CustomError> {
        return await this.api.put(`/content/location/${id}/name`, { name });
    }

    /**
     * Changes a location's code
     *
     * @param id The location id
     * @param code The new location code
     */
    async updateCode(id: string, code: string): Promise<void | CustomError> {
        return await this.api.put(`/content/location/${id}/code`, { name: code });
    }

    /**
     * Changes a location's boundry
     *
     * @param id The location id
     * @param name The new location boundry
     */
    async updateBoundry(id: string, boundry: Boundry): Promise<void | CustomError> {
        return await this.api.put(`/content/location/${id}/boundry`, { boundry });
    }
}
