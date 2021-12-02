import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { ExternalProvider, TwoFactorType } from '@shared/enums';
import { decodeJWT, getClaim } from '@shared/functions';
import { StorageOptions } from '@shared/interfaces';
import { CommSettings, ContentSettings, LoginResponse, SMap, User, UserStats } from '@shared/models';
import { StorageService } from '@shared/services';

export class AuthStore {
    saved_username: any;
    user: any;
    user_stats: any;
    comm_settings: any;
    content_settings: any;
    two_factor: any;
    expires: any;
    token: any;
    refresh_token: any;
    auth_token: any;
    provider: any;
    remember_me: any;
}

@Injectable({
    providedIn: 'root',
})
export class AuthState {
    private _loaded: boolean;
    roles: SMap<boolean>;
    is_mod: boolean;
    is_admin: boolean;

    authToken$: Observable<string>;

    constructor(public storage: StorageService<AuthStore>) {}

    async load() {
        if (this._loaded) return;

        await Promise.all([
            this.storage.load('saved_username'),
            this.storage.load('user'),
            this.storage.load('two_factor'),
            this.storage.load('provider'),
            this.storage.load('remember_me', { persisted: true }),
            this.storage.load('expires', { cookie: true }),
            this.storage.load<string>('token', { cookie: true }),
            this.storage.load('refresh_token', { cookie: true }),
            this.storage.load('auth_token', { cookie: true }),
            this.storage.load('comm_settings', { expiresIn: 15 }),
            this.storage.load('content_settings', { expiresIn: 15 }),
        ]);

        this.authToken$ = this.storage.watch('auth_token');
        this.storage.watch('token').subscribe((token: string) => {
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

        this._loaded = true;
    }

    get loggedIn(): boolean {
        return !!this.storage.get('token') && !!this.storage.get('refresh_token') && !!this.user;
    }

    get savedUsername(): string {
        return this.storage.get('saved_username');
    }
    set savedUsername(value: string) {
        this.storage.set('saved_username', value);
    }

    get commSettings(): CommSettings {
        return this.storage.get('comm_settings');
    }
    set commSettings(value: CommSettings) {
        this.storage.set('comm_settings', value);
    }

    get contentSettings(): ContentSettings {
        return this.storage.get('content_settings');
    }
    set contentSettings(value: ContentSettings) {
        this.storage.set('content_settings', value);
    }

    get user(): User {
        return this.storage.get('user');
    }
    set user(value: User) {
        this.storage.set('user', value);
    }

    get userStats(): UserStats {
        return this.storage.get('user_stats');
    }
    set userStats(value: UserStats) {
        this.storage.set('user_stats', value);
    }

    get twoFactorType(): TwoFactorType {
        return this.storage.get('two_factor');
    }

    get provider(): ExternalProvider {
        return this.storage.get('provider');
    }

    updateUser<T>(prop: keyof User, value: T) {
        const user = this.user;
        if (!user) return;

        user[prop as string] = value;
        this.user = user;
    }

    async authToken(): Promise<string> {
        return await this.storage.getFromStorage('auth_token');
    }

    removeAuthData() {
        this.storage.remove(
            'token',
            'refresh_token',
            'auth_token',
            'expires',
            'user',
            'two_factor',
            'provider',
            'remember_me',
            'comm_settings',
            'content_settings'
        );
    }

    async setLoginToken(data: LoginResponse) {
        if (!data) return;

        this.user = data.user;
        await Promise.all([
            this.storage.set('two_factor', data.twoFactorRequired),
            this.storage.set('provider', data.provider),
            this.setToken(data.token, data.refreshToken, data.authToken),
        ]);
    }

    private async setToken(token: string, refreshToken: string, authToken: string) {
        let expiryDate: Date = null;
        if (token != null) {
            try {
                const payload = decodeJWT(token);
                expiryDate = new Date((payload.exp - 15) * 1000);
            } catch (ex) {}
        }

        await Promise.all([
            this.storage.set('token', token, { cookie: true }),
            this.storage.set('expires', expiryDate, { cookie: true, expiresIn: expiryDate }),
            this.storage.set('refresh_token', refreshToken, { cookie: true }),
            this.storage.set('auth_token', authToken, { cookie: true }),
        ]);
    }

    async setRememberMe(state: boolean) {
        let date: Date;
        if (state) {
            date = new Date();
            date.setFullYear(date.getFullYear() + 1);
        }

        const opts: StorageOptions = { cookie: true, expiresIn: date };
        await Promise.all([
            this.storage.set('token', this.storage.get('token'), opts),
            this.storage.set('refresh_token', this.storage.get('refresh_token'), opts),
            this.storage.set('auth_token', this.storage.get('auth_token'), opts),
            this.storage.set('remember_me', state),
        ]);
    }
}
