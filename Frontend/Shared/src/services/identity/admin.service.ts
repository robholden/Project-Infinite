import { Injectable } from '@angular/core';

import { ContentSettings, CustomError } from '@shared/models';

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

    /**
     * Returns the settings for a provided user
     *
     * @param userId The userId to return the settings for
     */
    async getUserSettings(userId: string): Promise<ContentSettings | CustomError> {
        return await this.api.get<ContentSettings>(`/content/user/${userId}`);
    }

    /**
     * Updates the settings of a provided user
     *
     * @param userId The userId to update
     * @param settings The new settings to apply
     */
    async updateUserSettings(userId: string, settings: ContentSettings): Promise<void | CustomError> {
        return await this.api.put(`/content/user/${userId}`, settings);
    }
}
