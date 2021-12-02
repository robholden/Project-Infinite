import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { ExternalProvider, ProviderResult, RegisterExternalModal } from '@app/modals/register-external-modal/register-external.modal';

import { Facebook } from '@ionic-native/facebook/ngx';
import { GooglePlus } from '@ionic-native/google-plus/ngx';
import { AppleSignInErrorResponse, AppleSignInResponse, ASAuthorizationAppleIDRequest, SignInWithApple } from '@ionic-native/sign-in-with-apple/ngx';
import { LoadingController, ModalController } from '@ionic/angular';

import { CustomError } from '@shared/models';
import { AuthService } from '@shared/services/identity';

import { Device } from '@capacitor/device';

@Component({
    selector: 'sc-external-logins',
    templateUrl: './external-logins.component.html',
    styleUrls: ['./external-logins.component.scss'],
})
export class ExternalLoginsComponent implements OnInit {
    @Input() type: 'login' | 'register';
    @Input() ref: string;
    @Input() presentingElement?: HTMLElement;

    providers: ExternalProvider[] = ['google', 'facebook'];

    constructor(
        private authService: AuthService,
        private loadingCtrl: LoadingController,
        private modalCtrl: ModalController,
        private router: Router,
        private googlePlus: GooglePlus,
        private facebook: Facebook,
        private apple: SignInWithApple
    ) {}

    ngOnInit() {
        this.loadAppleSignIn();
    }

    async loginWithProvider(provider: ExternalProvider) {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Please wait',
            spinner: 'crescent',
        });
        await loading.present();

        // Trigger provider sign in
        const providerResult = await this.signInProvider(provider);
        if (!providerResult) return;

        // Verify token with our api
        const resp = await this.authService.loginWithExternalProvider(providerResult.provider, providerResult.token);
        if (resp instanceof CustomError) {
            await loading.dismiss();
            return;
        }

        // Close current modal
        if (await this.modalCtrl.getTop()) await this.modalCtrl.dismiss(null, null, this.type);
        await loading.dismiss();

        // If we have a token, then we're good to go
        if (!!resp.authToken) {
            await this.router.navigate([this.ref || this.router.url]);
        }

        // Register user with their external provider
        else {
            const modal = await this.modalCtrl.create({
                id: 'register-external',
                component: RegisterExternalModal,
                componentProps: { providerResult, user: resp.user },
                swipeToClose: true,
                presentingElement: this.presentingElement,
            });
            await modal.present();
        }
    }

    private async signInProvider(provider: ExternalProvider): Promise<ProviderResult> {
        // TODO: Secure with nonce tokens on the backend!
        switch (provider) {
            case 'google':
                try {
                    const user = await this.googlePlus.login({ webClientId: '862560448504-tuqume8imv1ueme6jkm91me52t138jm7.apps.googleusercontent.com' });
                    await this.googlePlus.logout();
                    return new ProviderResult('google', user.accessToken);
                } catch (ex) {
                    return null;
                }

            case 'facebook':
                try {
                    const fbResponse = await this.facebook.login(['public_profile', 'email']);
                    if (fbResponse.status === 'connected') {
                        this.facebook
                            .logout()
                            .then()
                            .catch(() => {});
                        return new ProviderResult('facebook', fbResponse.authResponse.accessToken);
                    }
                } catch (ex) {
                    return null;
                }

            case 'apple':
                try {
                    const result: AppleSignInResponse = await this.apple.signin({
                        requestedScopes: [
                            ASAuthorizationAppleIDRequest.ASAuthorizationScopeFullName,
                            ASAuthorizationAppleIDRequest.ASAuthorizationScopeEmail,
                        ],
                    });
                    return new ProviderResult('apple', result.identityToken);
                } catch (ex: AppleSignInErrorResponse | any) {
                    return null;
                }

            default:
                return null;
        }
    }

    private async loadAppleSignIn() {
        let device = await Device.getInfo();
        if (device.platform === 'ios') this.providers.push('apple');
    }
}
