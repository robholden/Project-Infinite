import { Injectable } from '@angular/core';

import { ContentSettings, CustomError } from '@shared/models';
import { AuthState } from '@shared/storage';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class ContentService {
    constructor(private api: HttpApiService, private authState: AuthState) {}

    /**
     * Gets the logged in user's content settings
     */
    async getSettings(): Promise<ContentSettings | CustomError> {
        if (this.authState.contentSettings) return this.authState.contentSettings;

        const settings = await this.api.get<ContentSettings>('/content/user', { toastError: false });
        if (settings instanceof CustomError) return settings;

        this.authState.contentSettings = settings;
        return settings;
    }

    /**
     * Updates the logged in user's content settings
     */
    async updateSettings(request: ContentSettings): Promise<void | CustomError> {
        return await this.api.put('/content/user', request, { toastError: false });
    }
}
