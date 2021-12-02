import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { ModalWithParams } from '@app/functions/setup-events.fn';

import { AlertController, LoadingController, ModalController, ToastController } from '@ionic/angular';

import { CustomEvent, TwoFactorType } from '@shared/enums';
import { CustomError, ErrorCode } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, PasswordService } from '@shared/services/identity';

import { TwoFactorModal } from '../two-factor-modal/two-factor.modal';

@Component({
    selector: 'sc-login',
    templateUrl: './login.modal.html',
    styleUrls: ['./login.modal.scss'],
})
export class LoginModal implements OnInit, ModalWithParams {
    form: FormGroup;
    googling: boolean;
    CustomEvent = CustomEvent;
    presentingElement?: HTMLIonModalElement | HTMLIonRouterOutletElement;

    @Input() ref: string;

    get username() {
        return this.form.get('username');
    }

    get password() {
        return this.form.get('password');
    }

    constructor(
        public events: EventService,
        private router: Router,
        private fb: FormBuilder,
        private authService: AuthService,
        private passwordService: PasswordService,
        public modalCtrl: ModalController,
        private toastCtrl: ToastController,
        private alertCtrl: AlertController,
        private loadingCtrl: LoadingController
    ) {
        this.setForm();
    }

    ngOnInit() {}

    async dismiss(fromUI: boolean = false) {
        const url = !fromUI && this.ref ? this.ref : this.router.url;

        await this.router.navigate([url]);
        await this.modalCtrl.dismiss('login');
    }

    async login() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Logging you in',
            spinner: 'crescent',
        });
        await loading.present();

        // Talk to our api and log them in
        const resp = await this.authService.login(this.username.value, this.password.value, { toastError: true });

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.IncorrectUsernameOrPassword) this.form.patchValue({ password: '' });
            await loading.dismiss();
            return;
        }

        // Check for 2FA
        if (!resp.user && resp.twoFactorRequired !== TwoFactorType.Unset) {
            const modal = await this.modalCtrl.create({
                id: '2FA',
                component: TwoFactorModal,
                componentProps: { type: resp.twoFactorRequired },
                swipeToClose: true,
                presentingElement: await this.modalCtrl.getTop(),
            });
            await loading.dismiss();
            await modal.present();
            const { data } = await modal.onDidDismiss();

            // If there's no user; they failed and quit - so shall we.
            if (!data) {
                this.form.patchValue({ password: '' });
                await this.authService.logout();
                return;
            }

            resp.user = data;
        }

        await loading.dismiss();
        await this.dismiss();
    }

    async forgotPassword() {
        // Show prompt
        const alert = await this.alertCtrl.create({
            header: 'Forgot Password',
            subHeader: 'Please enter your email address',
            inputs: [
                {
                    name: 'email',
                    type: 'email',
                    value: this.username.value,
                    attributes: {
                        inputmode: 'email',
                    },
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    cssClass: 'color-medium',
                },
                {
                    text: 'Submit',
                    cssClass: 'color-primary',
                    handler: async (inputs) => {
                        // Quit if email is null
                        if (!inputs.email) return false;

                        // Tell ui we're loading
                        const loading = await this.loadingCtrl.create({
                            message: 'Sending request',
                            spinner: 'crescent',
                        });
                        await loading.present();

                        // Call service
                        const resp = await this.passwordService.forgot(inputs.email);

                        // Stop if response is an exception
                        if (resp instanceof CustomError) {
                            await loading.dismiss();
                            return;
                        }

                        // Always passes
                        await loading.dismiss();
                        const toast = await this.toastCtrl.create({
                            message: 'If registered, you will receive an email with instructions on how to reset your password',
                            color: 'success',
                            duration: 5000,
                        });
                        await toast.present();
                    },
                },
            ],
        });

        await alert.present();
    }

    private setForm() {
        this.form = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(255)]],
            password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
        });
    }
}
