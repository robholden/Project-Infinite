import { Injectable } from '@angular/core';

import { CommSettings, CustomError } from '@shared/models';
import { HttpApiService } from '@shared/services';

@Injectable({
    providedIn: 'root',
})
export class CommsService {
    constructor(private api: HttpApiService) {}

    /**
     * Gets the logged in user's communication settings
     */
    async getSettings(): Promise<CommSettings | CustomError> {
        return await this.api.get('/comms/user', { toastError: false });
    }

    /**
     * Updates the logged in user's communication settings
     */
    async updateSettings(request: CommSettings): Promise<void | CustomError> {
        return await this.api.put('/comms/user', request, { toastError: false });
    }

    /**
     *  Unsubscribes a user by the key
     *
     * @param key Unique token
     */
    async unsubscribe(key: string): Promise<void | CustomError> {
        const resp = await this.api.put<void>('/comms/user/unsubscribe/' + key, {}, { toastError: false });
        if (resp instanceof CustomError) return resp;
    }
}
