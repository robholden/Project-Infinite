import { Injectable } from '@angular/core';

import { ContentFilters, CustomError } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class FiltersService {
    constructor(private api: HttpApiService) {}

    /**
     * Returns content filter values
     *
     */
    async get(): Promise<ContentFilters | CustomError> {
        return await this.api.get('/content/filters', { toastError: false });
    }
}
