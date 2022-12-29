import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { LoadingController, ModalController, ToastController } from '@ionic/angular';

import { PasswordStrength, zxcvbn } from '@shared/functions';
import { CustomError, ErrorCode } from '@shared/models';
import { PasswordService } from '@shared/services/identity';

@Component({
    selector: 'pi-change-password',
    templateUrl: './change-password.modal.html',
    styleUrls: ['./change-password.modal.scss'],
})
export class ChangePasswordModal implements OnInit {
    form: FormGroup;
    pwStrength: PasswordStrength;

    get oldPassword() {
        return this.form.get('oldPassword');
    }

    get newPassword() {
        return this.form.get('newPassword');
    }

    get confirmPassword() {
        return this.form.get('confirmPassword');
    }

    constructor(
        private fb: FormBuilder,
        private passwordService: PasswordService,
        private modalCtrl: ModalController,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController
    ) {
        this.setForm();
    }

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss(true);
    }

    async change() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Changing Password',
            spinner: 'crescent',
        });
        await loading.present();

        // Talk to our api and log them in
        const resp = await this.passwordService.change(this.oldPassword.value, this.newPassword.value);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.IncorrectPassword) this.form.patchValue({ oldPassword: '' });
            await loading.dismiss();
            return;
        }

        // Password reset successfully
        const toast = await this.toastCtrl.create({
            message: 'Password changed successfully',
            color: 'success',
            duration: 2500,
        });

        this.setForm();
        await loading.dismiss();
        await toast.present();
    }

    private setForm() {
        this.form = this.fb.group({
            oldPassword: ['', [Validators.required, Validators.minLength(6)]],
            newPassword: [
                '',
                [
                    Validators.required,
                    Validators.minLength(6),
                    Validators.maxLength(100),
                    (control: AbstractControl) => {
                        if (!this.form) return;

                        this.pwStrength = zxcvbn(control.value);
                        setTimeout(() => this.confirmPassword.updateValueAndValidity(), 0);
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
                        if (control.value !== this.newPassword.value) errors['mismatch'] = true;
                        return errors;
                    },
                ],
            ],
        });
    }
}
