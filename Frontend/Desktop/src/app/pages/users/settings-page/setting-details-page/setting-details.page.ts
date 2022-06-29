import { Component, OnInit } from '@angular/core';
import { Validators } from '@angular/forms';

import { maskEmail, toTitleCase } from '@shared/functions';
import { CustomError, Trx, User, UserField, userValidators } from '@shared/models';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';

@Component({
    selector: 'sc-setting-details',
    templateUrl: './setting-details.page.html',
    styleUrls: ['./setting-details.page.scss'],
})
export class SettingDetailsPage implements OnInit {
    sent: boolean;
    maskEmail: boolean = true;

    get email(): string {
        if (!this.user) return '';

        return this.maskEmail ? maskEmail(this.user.email) : this.user.email;
    }

    get user(): User {
        return this.authState.user;
    }

    constructor(
        public authState: AuthState,
        private userService: UserService,
        private alertCtrl: AlertController,
        private loadingCtrl: LoadingController
    ) {}

    ngOnInit() {}

    // Do edit field logic
    //
    async editField(field: UserField) {
        let message = '';
        if (field === 'email') message = `<p class="colour-danger">You will need to re-confirm any new email address</p>`;

        const result = await this.alertCtrl.create({
            title: `Update ${toTitleCase(field)}`,
            message,
            inputs: [
                {
                    name: 'value',
                    type: field === 'email' ? 'email' : 'text',
                    autocomplete: field,
                    validators: userValidators(field),
                    placeholder: `Enter your ${field}`,
                    value: this.user[field],
                    errorMap: { pattern: new Trx('form_errors.pattern', { name: 'Username' }) },
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Update',
                    role: 'submit',
                    colour: 'primary',
                    className: 'ml-a',
                },
            ],
        });

        if (!result) return;

        await this.update(field, result.value);
    }

    // Updates the user from api
    //
    async update(field: UserField, value: string) {
        // Stop if a request is in progress
        if (!this.hasChanged(field, value)) return;

        // Prompt for password when updating email or username
        let password: string = null;
        if (field === 'email' || field === 'username') {
            if (!!this.authState.provider) {
                password = '';
            } else {
                const result = await this.alertCtrl.create({
                    message: 'Your password is required to make this change',
                    inputs: [
                        {
                            name: 'password',
                            type: 'password',
                            autocomplete: 'new-password',
                            validators: [Validators.required, Validators.minLength(6)],
                        },
                    ],
                    buttons: [
                        {
                            text: 'Cancel',
                            role: 'cancel',
                            className: 'mr-a',
                        },
                        {
                            text: 'Update',
                            role: 'submit',
                            colour: 'primary',
                            className: 'ml-a',
                        },
                    ],
                });

                if (!result) return;

                password = result.password;
            }
        }

        // Call api
        const resp = await this.loadingCtrl.addBtnWithApi(`btn-${field}`, this.userService.update(field, value, { password }));
        if (!(resp instanceof CustomError) && field === 'email') this.user.emailConfirmed = false;
    }

    // Checks change in field
    //
    private hasChanged(field: UserField, value: string) {
        let oldValue = this.user[field] || '';
        let newValue = value || '';

        if (field !== 'name') {
            oldValue = oldValue.toLowerCase();
            newValue = newValue.toLowerCase();
        }

        return newValue !== oldValue;
    }

    // Resends a user their email confirmation
    //
    async resend() {
        if (this.sent) return;

        const confirmed = await this.alertCtrl.confirm({
            title: 'Resend Email',
            message: 'Do you want to resend your email confirmation?',
            confirmBtn: {
                text: 'Resend',
            },
        });
        if (!confirmed) return;

        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('resend-btn');
        loading.present();

        // Talk to api
        const resp = await this.userService.resendConfirmEmail();
        loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return;
        }

        // Email sent!
        this.sent = true;
    }
}
