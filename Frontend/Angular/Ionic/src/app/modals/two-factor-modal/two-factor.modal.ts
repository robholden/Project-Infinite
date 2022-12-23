import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AlertController, LoadingController, ModalController, ToastController } from '@ionic/angular';

import { TwoFactorType } from '@shared/enums';
import { CustomError, ErrorCode, User } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService } from '@shared/services/identity';

@Component({
    selector: 'sc-two-factor',
    templateUrl: './two-factor.modal.html',
    styleUrls: ['./two-factor.modal.scss'],
})
export class TwoFactorModal implements OnInit {
    @Input() type: TwoFactorType;
    TwoFactorType = TwoFactorType;

    form: FormGroup;

    get code() {
        return this.form.get('code');
    }

    get doNotAskAgain() {
        return this.form.get('doNotAskAgain');
    }

    constructor(
        public events: EventService,
        private fb: FormBuilder,
        private authService: AuthService,
        private modalCtrl: ModalController,
        private loadingCtrl: LoadingController,
        private alertCtrl: AlertController,
        private toastCtrl: ToastController
    ) {
        this.setForm();
    }

    ngOnInit() {}

    async dismiss(user: User = null) {
        await this.modalCtrl.dismiss(user, null, '2FA');
    }

    async verify() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Verifiying Code',
            spinner: 'crescent',
        });
        await loading.present();

        // Talk to our api and log them in
        const resp = await this.authService.verify(this.code.value, this.doNotAskAgain.value);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.ProvidedCodeInvalid) this.form.patchValue({ code: '' });
            await loading.dismiss();
            return;
        }

        // Close modal with success
        await this.dismiss(resp.user);
        await loading.dismiss();
    }

    async resend() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Resending Code',
            spinner: 'crescent',
        });
        await loading.present();

        // Go to api
        const resp = await this.authService.resend();
        await loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return;
        }

        // Resend successful
        const toast = await this.toastCtrl.create({
            message: 'A code has been successfully re-sent',
            color: 'success',
            duration: 2500,
        });
        await toast.present();
    }

    async recover() {
        // Show prompt
        const alert = await this.alertCtrl.create({
            header: 'Recover Account',
            subHeader: 'Enter one of your recovery codes',
            inputs: [
                {
                    name: 'code',
                    type: 'text',
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    cssClass: 'color-medium',
                },
                {
                    text: 'Recover',
                    cssClass: 'color-danger',
                    handler: async (inputs) => {
                        // Quit if email is null
                        if (!inputs.code) return;

                        // Tell ui we're loading
                        const loading = await this.loadingCtrl.create({
                            message: 'Recovering Account',
                            spinner: 'crescent',
                        });
                        await loading.present();

                        // Call service
                        const resp = await this.authService.recover(inputs.code);

                        // Stop if response is an exception
                        if (resp instanceof CustomError) {
                            await loading.dismiss();
                            return;
                        }

                        // Close modal with success
                        await this.dismiss(resp.user);
                        await loading.dismiss();
                    },
                },
            ],
        });

        await alert.present();
    }

    private setForm() {
        this.form = this.fb.group({
            code: ['', [Validators.required, Validators.pattern(/^[0-9]+$/), Validators.minLength(6), Validators.maxLength(6)]],
            doNotAskAgain: [false],
        });
    }
}
