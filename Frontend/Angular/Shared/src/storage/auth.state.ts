import { Injectable } from '@angular/core';

import { ExternalProvider, TwoFactorType } from '@shared/enums';
import { decodeJWT, getClaim } from '@shared/functions';
import { LoginResponse, SMap, User } from '@shared/models';

import { CustomStore } from './store';

export type AuthScheme = {
    savedUsername: string;
    user: User;
    twoFactor: TwoFactorType;
    expires: number;
    token: string;
    refreshToken: string;
    authToken: string;
    provider: ExternalProvider;
    rememberMe: boolean;
};

@Injectable({
    providedIn: 'root',
})
export class AuthState extends CustomStore<AuthScheme> {
    roles: SMap<boolean>;
    is_mod: boolean;
    is_admin: boolean;

    constructor() {
        super({
            savedUsername: { store: 'local' },
            authToken: { store: 'cookie' },
            token: { store: 'cookie' },
            refreshToken: { store: 'cookie' },
            expires: { store: 'cookie' },
            twoFactor: { store: 'session' },
            user: { store: 'session' },
            rememberMe: { store: 'cookie' },
        });

        this.observe('token').subscribe((token: string) => {
            const map: SMap<boolean> = {};

            if (token) {
                const roles = getClaim(token, 'role');
                if (typeof roles === 'string') map[roles] = true;
                else if (roles instanceof Array) roles.forEach((r) => (map[r] = true));
            }

            this.is_mod = map['Moderator'];
            this.is_admin = map['Admin'];
            this.roles = map;
        });
    }

    get loggedIn(): boolean {
        return !!this.snapshot('token') && !!this.snapshot('refreshToken') && !!this.snapshot('user');
    }

    removeAuthData() {
        this.removeMany('token', 'refreshToken', 'authToken', 'expires', 'user', 'twoFactor', 'provider', 'rememberMe');
    }

    async setLoginToken(data: LoginResponse) {
        if (!data) return;

        this.set('user', data.user).set('twoFactor', data.twoFactorRequired).set('provider', data.provider);
        this.setToken(data.token, data.refreshToken, data.authToken);
    }

    private async setToken(token: string, refreshToken: string, authToken: string) {
        let expiresIn: number = null;
        if (token != null) {
            try {
                const payload = decodeJWT(token);
                const expiryDate = new Date((payload.exp - 15) * 1000);
                expiresIn = expiryDate.getTime();
            } catch (ex) {}
        }

        this.set('expires', expiresIn, { expiresIn }).set('token', token).set('refreshToken', refreshToken).set('authToken', authToken);
    }

    async setRememberMe(state: boolean) {
        let expiresIn: Date;
        if (state) {
            expiresIn = new Date();
            expiresIn.setFullYear(expiresIn.getFullYear() + 1);
        }

        await Promise.all([
            this.set('token', this.snapshot('token'), { expiresIn }),
            this.set('refreshToken', this.snapshot('refreshToken'), { expiresIn }),
            this.set('authToken', this.snapshot('authToken'), { expiresIn }),
            this.set('rememberMe', state),
        ]);
    }
}
