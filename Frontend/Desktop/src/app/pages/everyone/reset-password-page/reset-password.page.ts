import { trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { LoadingController } from '@app/shared/controllers/loading';
import { ToastController } from '@app/shared/controllers/toast';

import { CustomEvent } from '@shared/enums';
import { PasswordStrength, zxcvbn } from '@shared/functions';
import { CustomError, ErrorCode } from '@shared/models';
import { EventService } from '@shared/services';
import { PasswordService } from '@shared/services/identity';

@Component({
    selector: 'sc-reset-password',
    templateUrl: './reset-password.page.html',
    styleUrls: ['./reset-password.page.scss'],
})
export class ResetPasswordPage implements OnInit {
    form: FormGroup;
    pwStrength: PasswordStrength;
    valid: boolean = null;

    private _key: string;

    constructor(
        private activatedRoute: ActivatedRoute,
        private fb: FormBuilder,
        private events: EventService,
        private passwordService: PasswordService,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController
    ) {
        // If there's no key; prompt for one
        const key = this.activatedRoute.snapshot.params['key'];
        this.validateKey(key);
    }

    ngOnInit() {}

    // Checks given key key is valid
    //
    async validateKey(key: string) {
        // Quit if key is null
        if (!key) {
            this.valid = false;
            return;
        }

        // Verify the key with the server
        const resp = await this.passwordService.validateResetKey(key);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            this.valid = false;
            return;
        }

        // We have a valid key
        this._key = key;
        this.setForm();
        this.valid = true;
    }

    // Attempts to reset password
    //
    async reset() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('reset-btn');
        loading.present();

        // Talk to our api and log them in
        const resp = await this.passwordService.reset(this._key, this.form.get('password').value, this.form.value.clear);

        // Stop loading
        loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.IncorrectPassword) this.form.patchValue({ password: '' });
            return;
        }

        // Password reset successfully
        const toast = this.toastCtrl.add('Password changed successfully', 'success');
        toast.present(5000);

        this.events.trigger(CustomEvent.Login);
    }

    // Sets the form data
    //
    private setForm() {
        this.form = this.fb.group({
            password: [
                '',
                [
                    Validators.required,
                    Validators.minLength(6),
                    Validators.maxLength(100),
                    (control: AbstractControl) => {
                        if (!this.form) return;

                        this.pwStrength = zxcvbn(control.value);
                        setTimeout(() => this.form.get('confirmPassword').updateValueAndValidity(), 0);
                    },
                ],
            ],
            confirmPassword: [
                '',
                [
                    Validators.required,
                    Validators.minLength(6),
                    (control: AbstractControl) => {
                        if (!this.form) return {};

                        const errors = {};
                        if (control.value !== this.form.get('password').value) errors['mismatch'] = true;
                        return errors;
                    },
                ],
            ],
            clear: [false],
        });
    }
}
