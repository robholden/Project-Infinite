import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { CustomEvent } from '@shared/enums';
import { INJ_TOAST } from '@shared/injectors';
import { Toast } from '@shared/interfaces';
import { CommSettings, ContentSettings, CustomError, SMap, User } from '@shared/models';
import { AppState, AuthState, AuthStore } from '@shared/storage';

import { EventService, HttpApiService, ReCaptchaService, SocketService } from './';
import { ContentService } from './content/content.service';
import { GatewayService } from './gateway';
import { StorageService } from './storage.service';

@Injectable({
    providedIn: 'root',
})
export class SiteLoaderService {
    healthy: boolean = null;

    constructor(
        @Inject(INJ_TOAST) private toastCtrl: Toast,
        private storage: StorageService<AuthStore>,
        private router: Router,
        private events: EventService,
        private gateway: GatewayService,
        private api: HttpApiService,
        private authState: AuthState,
        private appState: AppState,
        private sockets: SocketService,
        private contentService: ContentService,
        private recaptcha: ReCaptchaService
    ) {
        this.events.register(CustomEvent.RefreshToken, async () => await this.api.refreshToken());
    }

    async connectToServer() {
        this.healthy = null;

        // Load site keys
        const healthy = await this.gateway.healthy();
        if (!healthy) return;

        // Load recaptcha
        await this.recaptcha.load();
        this.healthy = healthy;

        // All good, start connections
        await this.connected();
    }

    async loadStorage() {
        await Promise.all([this.authState.load(), this.appState.load()]);
    }

    private async connected() {
        const err = (await this.api.refreshToken()) instanceof CustomError;
        if (err) {
            this.events.trigger(CustomEvent.Login, { ref: this.router.url });
            return;
        }

        this.initSocketEvents();

        if (this.authState.loggedIn) {
            await this.contentService.getSettings();
            this.sockets.connect();
        }
    }

    private initSocketEvents() {
        this.sockets.on('SessionRevoked', 'socket_service', async (hash) => {
            if (hash && hash !== (await this.storage.getFromStorage<string>('auth_token'))) return;
            else if (hash) this.toastCtrl.showError('Your session has been revoked', 5000);

            this.events.trigger(CustomEvent.Revoked);
        });

        this.sockets.on('UpdatedUserFields', 'socket_service', (newValues: SMap<any>) =>
            Object.keys(newValues).forEach((key: keyof User) => this.authState.updateUser(key, newValues[key]))
        );

        this.sockets.on('UpdatedUserSettings', 'socket_service', (settings: ContentSettings) => (this.authState.contentSettings = settings));
        this.sockets.on('UpdatedUserPreferences', 'socket_service', (settings: CommSettings) => (this.authState.commSettings = settings));
        this.sockets.on('InvalidateUser', 'socket_service', async () => await this.storage.removeFromStorage('user'));

        this.sockets.on('GroupValue', 'socket_service', async (value: string) => await this.toastCtrl.showMessage(value, 5000));
    }
}
