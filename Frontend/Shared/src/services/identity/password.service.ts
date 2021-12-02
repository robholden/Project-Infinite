import { Injectable } from '@angular/core';

import { CustomError } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class PasswordService {
    constructor(private api: HttpApiService) {}

    /**
     * Attempts to send email with reset link
     *
     * @param email the user's email address
     */
    async forgot(email: string): Promise<void | CustomError> {
        // Go to api
        const resp = await this.api.post<void>('/identity/password/forgot', { email });

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;
    }

    /**
     * Validates a given reset password key
     *
     * @param key The key to verify
     */
    async validateResetKey(key: string): Promise<void | CustomError> {
        // Go to api
        const resp = await this.api.get<void>('/identity/password/reset/' + key, { toastError: false });

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;
    }

    /**
     * Resets a user's password
     *
     * @param key The key for resetting
     * @param password The user's new password
     * @param clear Flag to reset all other sessions
     */
    async reset(key: string, password: string, clear: boolean): Promise<void | CustomError> {
        // Go to api
        const resp = await this.api.put<void>(
            '/identity/password/reset/' + key,
            { password, clear },
            { recaptchaAction: 'password_reset', toastError: false }
        );

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;
    }

    /**
     * Changes a user's password
     *
     * @param oldPassword User's new password
     * @param newPassword User's old password
     */
    async change(oldPassword: string, newPassword: string): Promise<void | CustomError> {
        // Go to api
        const resp = await this.api.put<void>('/identity/password/change', { oldPassword, newPassword }, { recaptchaAction: 'password_change' });

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;
    }
}
