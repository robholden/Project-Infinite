import { Injectable } from '@angular/core';

import { Subject } from 'rxjs';

import { ToastController } from '@app/shared/controllers/toast';

import { environment } from '@env/environment';

import { loadScripts } from '@shared/functions';
import { Provider, ProviderResult, SMap, SocialUser } from '@shared/models';

import { v4 as uuidv4 } from 'uuid';

declare const AppleID: any;
declare const google: any;
declare const FB: any;

@Injectable({
    providedIn: 'root',
})
export class LoginProvidersService {
    private state: string;
    private disabled: SMap<boolean> = {};
    private googleListener = new Subject<SocialUser>();
    private googleListener$ = this.googleListener.asObservable();

    constructor(private toastCtrl: ToastController) {}

    async init() {
        await loadScripts(
            { src: 'https://appleid.cdn-apple.com/appleauth/static/jsapi/appleid/1/en_US/appleid.auth.js' },
            { src: 'https://accounts.google.com/gsi/client' },
            { src: 'https://connect.facebook.net/en_US/sdk.js' }
        );

        this.state = uuidv4();

        try {
            google.accounts.id.initialize({
                client_id: environment.google.clientId,
                callback: (result: any) => this.googleListener.next(new SocialUser(result?.credential)),
            });
        } catch (err) {
            this.disabled['google'] = true;
        }

        try {
            AppleID.auth.init({
                clientId: environment.apple.clientId,
                scope: 'name email',
                redirectURI: environment.apple.redirectUrl,
                state: this.state,
                usePopup: true,
            });
        } catch (err) {
            this.disabled['apple'] = true;
        }

        try {
            FB.init({
                appId: environment.facebook.appId,
                autoLogAppEvents: false,
                xfbml: false,
                version: 'v11.0',
            });
        } catch (err) {
            this.disabled['facebook'] = true;
        }
    }

    async login(provider: Provider): Promise<ProviderResult> {
        if (this.disabled[provider]) {
            this.toastCtrl.add(`Could not load library. This could be due to Tracking Prevention filters.`, 'danger').present(5000);
            return null;
        }

        let user: SocialUser = null;
        switch (provider) {
            case 'apple':
                user = await this.appleToken();
                break;

            case 'google':
                user = await this.googleToken();
                break;

            case 'facebook':
                user = await this.facebookToken();
                break;
        }

        if (!user) return null;
        else return new ProviderResult(provider, user);
    }

    private async appleToken(): Promise<SocialUser> {
        try {
            const data = await AppleID.auth.signIn();
            const auth = data?.authorization;
            if (auth?.state !== this.state || !auth?.id_token) return null;

            return new SocialUser(auth.id_token, data.user?.name?.firstName);
        } catch (err) {
            return null;
        }
    }

    private async facebookToken(): Promise<SocialUser> {
        return await new Promise<SocialUser>((res) => {
            FB.login((response: any) => {
                const token = response?.authResponse?.accessToken;
                if (token) res(new SocialUser(token));
                else res(null);
            });
        });
    }

    private async googleToken(): Promise<SocialUser> {
        return await new Promise<SocialUser>((res) => {
            // Reset cooldown
            document.cookie = 'g_state=; Max-Age=-99999999;';

            let user: SocialUser = null;
            this.googleListener$.subscribe((socialUser) => (user = socialUser));

            google.accounts.id.prompt((notification: any) => {
                const notDisplayed = notification.isNotDisplayed();

                if (notDisplayed && notification.j === 'suppressed_by_user') {
                    this.toastCtrl.add(`Cannot login with Google, a user cooldown is in affect`, 'warning').present(5000);
                }

                if (notDisplayed || notification.isSkippedMoment()) res(null);
                else if (user) res(user);
            });
        });
    }
}
