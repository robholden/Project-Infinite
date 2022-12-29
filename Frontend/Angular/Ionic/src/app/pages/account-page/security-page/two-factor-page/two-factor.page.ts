import { Component, OnInit } from '@angular/core';

import { promptForPassword } from '@app/functions/password-prompt.fn';

import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { AlertController, IonRouterOutlet, LoadingController, ModalController, ToastController } from '@ionic/angular';
import { Storage } from '@ionic/storage-angular';

import { TwoFactorType } from '@shared/enums';
import { CustomError, PasswordRequest } from '@shared/models';
import { AuthService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

import { SetMobileModal } from './modals/set-mobile/set-mobile.modal';
import { SetupTwoFactorModal } from './modals/setup-two-factor/setup-two-factor.modal';

@Component({
    selector: 'pi-two-factor',
    templateUrl: './two-factor.page.html',
    styleUrls: ['./two-factor.page.scss'],
})
export class TwoFactorPage implements OnInit {
    get usingAny(): boolean {
        return this.authState.user.twoFactorEnabled && this.authState.user.twoFactorType !== TwoFactorType.Unset;
    }

    get usingApp(): boolean {
        return this.authState.user.twoFactorEnabled && this.authState.user.twoFactorType === TwoFactorType.App;
    }

    get usingEmail(): boolean {
        return this.authState.user.twoFactorEnabled && this.authState.user.twoFactorType === TwoFactorType.Email;
    }

    get usingSms(): boolean {
        return this.authState.user.twoFactorEnabled && this.authState.user.twoFactorType === TwoFactorType.Sms;
    }

    constructor(
        public authState: AuthState,
        private routerOutlet: IonRouterOutlet,
        private authService: AuthService,
        private toastCtrl: ToastController,
        private alertCtrl: AlertController,
        private modalCtrl: ModalController,
        private loadingCtrl: LoadingController,
        private fingerprint: FingerprintAIO,
        private storage: Storage
    ) {}

    ngOnInit() {}

    async setupApp() {
        await this.setupPrompt(TwoFactorType.App);
    }

    async setupEmail() {
        await this.setupPrompt(TwoFactorType.Email);
    }

    async setupSms() {
        await this.setupPrompt(TwoFactorType.Sms);
    }

    private async setupPrompt(type: TwoFactorType) {
        const passwordRequest = await promptForPassword(
            {
                title: 'Setup 2FA',
                touchId: { message: 'Scan your fingerprint to begin the setup' },
                password: { message: 'Enter your password to begin the setup', submitText: 'Setup' },
            },
            this.alertCtrl,
            this.fingerprint,
            this.storage,
            this.authState
        );
        if (passwordRequest === null) return;

        // Prompt for mobile number when choose SMS
        let mobile = '';
        if (type === TwoFactorType.Sms) {
            mobile = await this.requestMobile();
            if (!mobile) return false;
        }

        await this.setup(type, passwordRequest, mobile);
    }

    private async requestMobile(): Promise<string> {
        const modal = await this.modalCtrl.create({
            id: 'set-mobile',
            component: SetMobileModal,
            swipeToClose: true,
            presentingElement: this.routerOutlet.nativeEl,
        });
        await modal.present();
        const { data } = await modal.onDidDismiss();

        return data;
    }

    private async setup(type: TwoFactorType, passwordRequest: PasswordRequest, mobile?: string) {
        // Quit if password is empty
        if (!passwordRequest) return;

        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Verifying',
            spinner: 'crescent',
        });
        await loading.present();

        // Call api
        const resp = await this.authService.setup2FA(type, passwordRequest, mobile);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            await loading.dismiss();
            return;
        }

        const modal = await this.modalCtrl.create({
            component: SetupTwoFactorModal,
            componentProps: {
                type,
                data: resp,
                email: this.authState.user.email,
            },
            swipeToClose: true,
            presentingElement: this.routerOutlet.nativeEl,
        });
        await modal.present();
        await loading.dismiss();
    }

    async remove() {
        await this.disable2FA();
    }

    private async disable2FA() {
        const alert = await this.alertCtrl.create({
            header: 'Disable 2FA',
            subHeader: 'Your password is required to remove two-factor authentication from your account',
            inputs: [
                {
                    name: 'password',
                    type: 'password',
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    cssClass: 'color-medium',
                },
                {
                    text: 'Remove',
                    cssClass: 'color-danger',
                    handler: async (inputs) => {
                        if (!inputs.password) return;

                        // Tell ui we're loading
                        const loading = await this.loadingCtrl.create({
                            message: 'Removing',
                            spinner: 'crescent',
                        });
                        await loading.present();

                        const resp = await this.authService.disable2FA(inputs.password);
                        if (resp instanceof CustomError) {
                            await loading.dismiss();
                            return;
                        }

                        const toast = await this.toastCtrl.create({
                            message: 'Two-factor authentication has been removed',
                            color: 'primary',
                            duration: 2500,
                        });

                        await loading.dismiss();
                        await toast.present();
                    },
                },
            ],
        });

        await alert.present();
    }
}
