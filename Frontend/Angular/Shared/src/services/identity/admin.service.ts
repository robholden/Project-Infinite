import { Injectable } from '@angular/core';

import { CustomError } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class AdminService {
    constructor(private api: HttpApiService) {}

    /**
     * Resets all user's terms accept flag and will prompt them with accept terms modal
     */
    async resetTermsForAllUsers(): Promise<void | CustomError> {
        return await this.api.post('/identity/admin/reset-terms', {});
    }
}
