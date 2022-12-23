import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { ModalWithParams } from '@app/functions/setup-events.fn';

import { LoadingController, ModalController, ToastController } from '@ionic/angular';

import { CustomEvent } from '@shared/enums';
import { PasswordStrength, zxcvbn } from '@shared/functions';
import { CustomError, userValidators } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, UserService } from '@shared/services/identity';

@Component({
    selector: 'sc-register',
    templateUrl: './register.modal.html',
    styleUrls: ['./register.modal.scss'],
})
export class RegisterModal implements OnInit, ModalWithParams {
    ref: string;
    form: FormGroup;
    pwStrength: PasswordStrength;
    CustomEvent = CustomEvent;
    presentingElement?: HTMLIonModalElement | HTMLIonRouterOutletElement;

    get name() {
        return this.form.get('name');
    }

    get email() {
        return this.form.get('email');
    }

    get username() {
        return this.form.get('username');
    }

    get password() {
        return this.form.get('password');
    }

    get confirmPassword() {
        return this.form.get('confirmPassword');
    }

    get terms() {
        return this.form.get('terms');
    }

    constructor(
        public events: EventService,
        private router: Router,
        private fb: FormBuilder,
        private userService: UserService,
        private authService: AuthService,
        private modalCtrl: ModalController,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController
    ) {
        this.setForm();
    }

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss('register');
    }

    async register() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Registering',
            spinner: 'crescent',
        });
        await loading.present();

        // Talk to our api and register them
        const regResp = await this.userService.register(this.name.value, this.email.value, this.username.value, this.password.value, { toastError: true });

        // Stop if response is an exception
        if (regResp instanceof CustomError) {
            await loading.dismiss();
            return;
        }

        // Login user
        const loginResp = await this.authService.login(regResp.email, this.password.value);

        // Stop if response is an exception
        if (loginResp instanceof CustomError) {
            await loading.dismiss();
            return;
        }

        // Registration successful
        const toast = await this.toastCtrl.create({
            message: `Thanks for registering ${regResp.name}!`,
            color: 'primary',
            duration: 2500,
        });

        await this.router.navigate([this.ref ? this.ref : '/']);
        await this.dismiss();
        await loading.dismiss();
        await toast.present();
    }

    private setForm() {
        this.form = this.fb.group({
            name: ['', userValidators('name')],
            email: ['', userValidators('email')],
            username: ['', userValidators('username')],
            terms: [false, [Validators.requiredTrue]],
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
                    Validators.maxLength(100),
                    (control: AbstractControl) => {
                        if (!this.form) return {};

                        const errors = {};
                        if (control.value !== this.form.get('password').value) errors['mismatch'] = true;
                        return errors;
                    },
                ],
            ],
        });
    }
}
