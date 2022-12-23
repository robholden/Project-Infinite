import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { CustomEvent } from '@shared/enums';
import { INJ_TOAST } from '@shared/injectors';
import { Toast } from '@shared/interfaces';
import { CustomError, SMap, User } from '@shared/models';
import { AppState, AuthState } from '@shared/storage';

import { EventService, HttpApiService, ReCaptchaService, SocketService } from './';
import { GatewayService } from './gateway';

@Injectable({
    providedIn: 'root',
})
export class SiteLoaderService {
    healthy: boolean = null;

    constructor(
        @Inject(INJ_TOAST) private toastCtrl: Toast,
        private storage: AuthState,
        private router: Router,
        private events: EventService,
        private gateway: GatewayService,
        private api: HttpApiService,
        private authState: AuthState,
        private appState: AppState,
        private sockets: SocketService,
        private recaptcha: ReCaptchaService
    ) {
        this.events.register(CustomEvent.RefreshToken, async () => await this.api.refreshToken());
    }

    async connectToServer() {
        this.healthy = null;

        // Load site keys
        this.healthy = await this.gateway.healthy();
        if (!this.healthy) return;

        // Load recaptcha
        await this.recaptcha.load();

        // All good, start connections
        await this.connected();
    }

    async loadStorage() {
        await Promise.all([this.authState.init(), this.appState.init()]);
    }

    private async connected() {
        const err = (await this.api.refreshToken()) instanceof CustomError;
        if (err) {
            this.events.trigger(CustomEvent.Login, { ref: this.router.url });
            return;
        }

        this.initSocketEvents();

        if (this.authState.loggedIn) {
            this.sockets.connect();
        }
    }

    private initSocketEvents() {
        this.sockets.on('SessionRevoked', 'socket_service', async (hash) => {
            if (hash && hash !== (await this.storage.retrieve('authToken'))) return;
            else if (hash) this.toastCtrl.showError('Your session has been revoked', 5000);

            this.events.trigger(CustomEvent.Revoked);
        });

        this.sockets.on('UpdatedUserFields', 'socket_service', (newValues: SMap<any>) =>
            Object.keys(newValues).forEach((key: keyof User) =>
                this.authState.update('user', (user) => {
                    (user as any)[key] = newValues[key];
                    return user;
                })
            )
        );

        this.sockets.on('InvalidateUser', 'socket_service', async () => await this.storage.remove('user'));
        this.sockets.on('GroupValue', 'socket_service', async (value: string) => await this.toastCtrl.showMessage(value, 5000));
    }
}
