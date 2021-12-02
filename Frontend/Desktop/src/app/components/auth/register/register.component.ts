import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { LoadingController } from '@app/shared/controllers/loading/loading.controller';

import { PasswordStrength, zxcvbn } from '@shared/functions';
import { CustomError, Trx, userValidators } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, UserService } from '@shared/services/identity';

@Component({
    selector: 'sc-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
    form: FormGroup;
    pwStrength: PasswordStrength;
    regError: Trx;
    loginError: Trx;

    @Output() completed = new EventEmitter<boolean>();

    constructor(
        public events: EventService,
        private fb: FormBuilder,
        private userService: UserService,
        private authService: AuthService,
        private loadingCtrl: LoadingController
    ) {
        this.setForm();
    }

    ngOnInit() {
        setTimeout(() => document.getElementById('name').focus(), 0);
    }

    async register() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('register-btn');
        loading.present();
        this.regError = null;
        this.loginError = null;

        // Talk to our api and register them
        const regResp = await this.userService.register(
            this.form.get('name').value,
            this.form.get('email').value,
            this.form.get('username').value,
            this.form.get('password').value
        );

        // Stop if response is an exception
        if (regResp instanceof CustomError) {
            loading.dismiss();
            this.regError = regResp;
            return;
        }

        // Login user
        const loginResp = await this.authService.login(regResp.email, this.form.get('password').value);

        // Hide loading
        loading.dismiss();

        // Stop if response is an exception
        if (loginResp instanceof CustomError) {
            this.loginError = loginResp;
            return;
        }

        this.completed.emit();
    }

    // Sets the form data
    //
    private setForm() {
        this.form = this.fb.group({
            name: ['rob', userValidators('name')],
            email: ['rob.holden@live.co.uk', userValidators('email')],
            username: ['admin', userValidators('username')],
            terms: [false, [Validators.requiredTrue]],
            password: [
                'admin1',
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
                'admin1',
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
