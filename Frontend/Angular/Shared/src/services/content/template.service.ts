import { Injectable } from '@angular/core';

import { CustomError, TemplateKey } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class TemplateService {
    constructor(private api: HttpApiService) {}

    /**
     * Returns all available template keys
     */
    async keys(): Promise<TemplateKey[] | CustomError> {
        return await this.api.get<TemplateKey[]>(`/content/template`);
    }
}
