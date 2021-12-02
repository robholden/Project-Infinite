import { Injectable } from '@angular/core';

import { TwoFactorType } from '@shared/enums';
import { obj2QueryString } from '@shared/functions';
import { CustomError, ErrorCode, LoginResponse, PagedList, PageRequest, PasswordRequest, RecoveryCode, Session } from '@shared/models';
import { AuthState } from '@shared/storage';

import { HttpApiService, SocketService } from '../';
import { ContentService } from '../content';
import { ApiOptions } from '../http-api.service';

export interface TwoFactorResponse {
    secret: string;
    mode: string;
    size: number;
    step: number;
}

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    constructor(private api: HttpApiService, private state: AuthState, private sockets: SocketService, private contentService: ContentService) {}

    /**
     * Creates a token with credentials
     *
     * @param username The user's username or email
     * @param password The user's password
     */
    async login(username: string, password: string, apiOpts?: ApiOptions<LoginResponse>): Promise<LoginResponse | CustomError> {
        // Call request
        const resp = await this.api.post<LoginResponse>(
            '/identity/auth/login',
            { username, password },
            { recaptchaAction: 'login', toastError: false, skipRefresh: true, ...(apiOpts || {}) }
        );

        // Update temp
        this.state.savedUsername = username;

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Store auth data
        await this.state.setLoginToken(resp);

        // Complete login process when no using 2FA
        if (resp.twoFactorRequired === TwoFactorType.Unset) this.completeLogin();

        return resp;
    }

    /**
     * Logins user in with provider
     */
    async loginWithExternalProvider(provider: string, token: string, name?: string): Promise<LoginResponse | CustomError> {
        // Call request
        const resp = await this.api.post<LoginResponse>(
            '/identity/auth/login/' + provider,
            { token, name },
            { recaptchaAction: 'login_' + provider, skipRefresh: true }
        );

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Store auth data
        await this.state.setLoginToken(resp);

        // Complete login process
        if (resp.refreshToken) this.completeLogin();

        return resp;
    }

    /**
     * Logins user in with touch id
     */
    async loginWithTouchId(key: string): Promise<LoginResponse | CustomError> {
        // Call request
        const resp = await this.api.post<LoginResponse>(
            '/identity/auth/login/touch-id',
            { key },
            { recaptchaAction: 'login_touch', toastError: false, skipRefresh: true }
        );

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Store auth data
        await this.state.setLoginToken(resp);

        // Complete login process
        if (resp.refreshToken) this.completeLogin();

        return resp;
    }

    /**
     * Verifies a 2FA code to complete the login process
     *
     * @param code 6 digit code
     * @param doNotAskAgain To not ask again for this device
     */
    async verify(code: string, doNotAskAgain: boolean): Promise<LoginResponse | CustomError> {
        // Go to api
        const resp = await this.api.post<LoginResponse>('/identity/auth/verify', { code, doNotAskAgain }, { recaptchaAction: '2fa_submit' });

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Store auth data
        await this.state.setLoginToken(resp);

        // Complete login process
        this.completeLogin();

        return resp;
    }

    /**
     * Resend a 2FA code to complete the login process
     */
    async resend(): Promise<void | CustomError> {
        // Go to api
        const resp = await this.api.post<any>('/identity/auth/resend-code');

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;
    }

    /**
     * Logs out the current user
     */
    async logout(local?: boolean): Promise<void | CustomError> {
        // Go to api
        if (!local) {
            const resp = await this.api.get<any>('/identity/auth/logout', { toastError: false, skipRefresh: true });

            // Quit if we have errors
            if (resp instanceof CustomError && resp.code !== ErrorCode.SessionHasExpired) {
                return resp;
            }
        }

        // Clear auth data
        this.state.removeAuthData();

        // Stop web socket
        this.sockets.stop();
    }

    /**
     * Recovers a user's account
     *
     * @param code the recovery code
     */
    async recover(code: string): Promise<LoginResponse | CustomError> {
        // Go to api
        const resp = await this.api.post<LoginResponse>('/identity/auth/recover-account', { code }, { recaptchaAction: 'recover_account' });

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Store auth data
        this.state.setLoginToken(resp);

        return resp;
    }

    /**
     * Sets up two-factor authentication
     *
     * @param type The type of 2FA
     * @param passwordRequest Password request details
     * @param mobile The user's mobile, when choosing SMS 2FA
     */
    async setup2FA(type: TwoFactorType, passwordRequest: PasswordRequest, mobile?: string): Promise<TwoFactorResponse | CustomError> {
        // Go to api
        const resp = await this.api.post<TwoFactorResponse>(
            '/identity/2fa/setup',
            {
                ...passwordRequest,
                command: { type: TwoFactorType[type], mobile },
            },
            { recaptchaAction: '2fa_setup' }
        );

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Return secret
        return resp;
    }

    /**
     * Completes the 2FA setup
     *
     * @param type The type of 2FA
     * @param code The 2FA code to complete setup
     */
    async enable2FA(type: TwoFactorType, code: string): Promise<RecoveryCode[] | CustomError> {
        // Go to api
        const resp = await this.api.post<RecoveryCode[]>('/identity/2fa/enable', { code }, { recaptchaAction: '2fa_enable' });

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Add 2fa data to auth user
        this.state.updateUser('twoFactorEnabled', true);
        this.state.updateUser('twoFactorType', type);

        return resp;
    }

    /**
     * Remove two-factor authentication
     *
     * @param passwordRequest Password request details
     */
    async disable2FA(passwordRequest: PasswordRequest): Promise<void | CustomError> {
        // Go to api
        const resp = await this.api.post<any>('/identity/2fa/disable', passwordRequest, { recaptchaAction: '2fa_disable' });

        // Quit if we have errors
        if (resp instanceof CustomError) return resp;

        // Wipe 2fa from auth user
        this.state.updateUser('twoFactorEnabled', false);
        this.state.updateUser('twoFactorType', TwoFactorType.Unset);
    }

    /**
     * A list of the logged in user's sessions
     *
     * @param pageRequest Page options
     */
    async getSessions(pageRequest: PageRequest): Promise<PagedList<Session> | CustomError> {
        return await this.api.get<PagedList<Session>>('/identity/auth/sessions?' + obj2QueryString(pageRequest));
    }

    /**
     * Delete a session
     *
     * @param guid Session id
     * @param passwordRequest Password request details
     */
    async deleteSession(guid: string, passwordRequest: PasswordRequest): Promise<void | CustomError> {
        return await this.api.post<void>(`/identity/auth/session/${guid}/delete`, passwordRequest);
    }

    /**
     * Deletes all session (including theirs)
     *
     * @param passwordRequest Password request details
     */
    async deleteAll(passwordRequest: PasswordRequest): Promise<void | CustomError> {
        return await this.api.post<void>('/identity/auth/session/delete', passwordRequest);
    }

    /**
     * Enables or disables touch id
     *
     * @param enabled Whether to enable/disable touch id
     */
    async enableDisableTouchId(enabled: boolean): Promise<boolean> {
        const resp = await this.api.post<void>('/identity/auth/touch-id/' + (enabled ? 'enabled' : 'disabled'));
        return !(resp instanceof CustomError);
    }

    private async completeLogin() {
        this.sockets.connect();
        await this.contentService.getSettings();
    }
}
