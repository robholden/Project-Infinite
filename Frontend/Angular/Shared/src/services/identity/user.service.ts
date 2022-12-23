import { Injectable } from '@angular/core';

import { CustomError, PasswordRequest, ReportUserReason, User, UserField } from '@shared/models';
import { AuthState } from '@shared/storage';

import { ApiOptions, HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class UserService {
    constructor(private api: HttpApiService, private authState: AuthState) {}

    /**
     * Gets a user by a username
     *
     * @param username The username
     */
    async get(username: string): Promise<User | CustomError> {
        return await this.api.get('/identity/user/' + username, { toastError: false });
    }

    /**
     * Gets a user by their id
     *
     * @param id The user id
     */
    async getById(id: string): Promise<User | CustomError> {
        return await this.api.get('/identity/user/id/' + id, { toastError: false });
    }

    /**
     * Updates a specific user field
     *
     * @param field Which field to update
     * @param value The value to change to
     * @param passwordRequest Password request details
     */
    async update<T>(field: UserField, value: T, passwordRequest: PasswordRequest = null): Promise<void | CustomError> {
        // Go to api
        const command = { [field]: value };
        const user = await this.api.put<void>(
            '/identity/user/update/' + (field ? field : ''),
            passwordRequest?.password ? { ...passwordRequest, command } : command,
            {
                recaptchaAction: 'user_update_' + field,
            }
        );

        // Quit if we have errors
        if (user instanceof CustomError) return user;

        // Update field
        this.authState.update('user', (user) => {
            (user as any)[field] = value;
            return user;
        });
    }

    /**
     * Registers a new user
     *
     * @param name Their name
     * @param email Their email address
     * @param username Their username
     * @param password Their password
     */
    async register(name: string, email: string, username: string, password: string, apiOpts?: ApiOptions<User>): Promise<User | CustomError> {
        return await this.api.post<User>(
            '/identity/user',
            { name, email, username, password },
            { recaptchaAction: 'register', toastError: false, skipRefresh: true, ...(apiOpts || {}) }
        );
    }

    /**
     * Registers a new user via an external provider
     *
     * @param provider The external provider
     * @param name Their name
     * @param username Their username
     */
    async registerExternalProvider(provider: string, name: string, username: string, apiOpts?: ApiOptions<User>): Promise<User | CustomError> {
        return await this.api.post<User>(
            '/identity/user/register/' + provider,
            { provider, name, username },
            { recaptchaAction: 'register_' + provider, toastError: false, skipRefresh: true, ...(apiOpts || {}) }
        );
    }

    /**
     * Confirms a user's email by the key
     *
     * @param key Unique token
     */
    async confirmEmail(key: string): Promise<void | CustomError> {
        const resp = await this.api.put<void>('/identity/user/confirm-email/' + key, null, { toastError: false, recaptchaAction: 'confirm_email' });
        if (resp instanceof CustomError) return resp;

        if (this.authState.loggedIn) {
            this.authState.update('user', (user) => ({ ...user, emailConfirmed: true }));
        }
    }

    /**
     * Resends the confirmation email to the logged in user
     */
    async resendConfirmEmail(): Promise<void | CustomError> {
        const resp = await this.api.put<void>('/identity/user/resend-confirm-email', {});
        if (resp instanceof CustomError) return resp;
    }

    /**
     * Deletes the logged in user's account
     *
     * @param password The user's password
     */
    async deleteAccount(password: string): Promise<void | CustomError> {
        const resp = await this.api.post<User>('/identity/user/account/delete', { password }, { recaptchaAction: 'delete_account' });
        if (resp instanceof CustomError) return resp;

        // Bind user to state
        this.authState.removeAuthData();
    }

    /**
     * Reports the given user
     *
     * @param username The username
     * @param reason The reported reason
     */
    async report(username: string, reason: ReportUserReason): Promise<void | CustomError> {
        return await this.api.post(`/identity/user/${username}/report`, { reason });
    }

    /**
     *  Unsubscribes a user by the key
     *
     * @param key Unique token
     */
    async unsubscribe(key: string): Promise<void | CustomError> {
        const resp = await this.api.put<void>('/identity/user/unsubscribe/' + key, {}, { toastError: false });
        if (resp instanceof CustomError) return resp;
    }
}
