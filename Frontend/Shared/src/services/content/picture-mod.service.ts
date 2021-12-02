import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { CustomError, PagedList, PageRequest, PictureModeration, PictureModSearch } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class PictureModService {
    constructor(private api: HttpApiService) {}

    /**
     * Searches pictures by given parameters
     *
     * @param route The api route to use for the endpoint
     * @param search The search criteria
     * @param pageRequest Page options
     */
    async lookup(route: string, search: PictureModSearch, pageRequest: PageRequest): Promise<PagedList<PictureModeration> | CustomError> {
        return await this.api.get<PagedList<PictureModeration>>(`/content/picture/mod/search${route || ''}?` + obj2QueryString(search, pageRequest), {
            toastError: false,
        });
    }

    /**
     * Get the next available picture to moderate
     */
    async next(): Promise<string | CustomError> {
        return await this.api.get<string>('/content/picture/mod/next', {
            toastError: false,
        });
    }

    /**
     * Submitted picture moderation result
     *
     * @param id the report id
     * @param outcome whether the report is approved or not
     * @param notes info about the moderation
     */
    async action(id: string, outcome: boolean, notes: string = ''): Promise<void | CustomError> {
        return await this.api.post(`/content/picture/mod/${id}`, { outcome, notes });
    }
}
