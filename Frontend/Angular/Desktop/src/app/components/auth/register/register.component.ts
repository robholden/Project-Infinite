import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';

import { PasswordStrength, zxcvbn } from '@shared/functions';
import { CustomError, Trx, userValidators } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, UserService } from '@shared/services/identity';

import { LoadingController } from '@app/shared/controllers/loading/loading.controller';

@Component({
    selector: 'pi-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
    pwStrength: PasswordStrength;
    regError: Trx;
    loginError: Trx;
    form = new FormGroup({
        name: new FormControl('rob', userValidators('name')),
        email: new FormControl('rob.holden@live.co.uk', userValidators('email')),
        username: new FormControl('admin', userValidators('username')),
        terms: new FormControl(false, [Validators.requiredTrue]),
        password: new FormControl('admin1', [
            Validators.required,
            Validators.minLength(6),
            Validators.maxLength(100),
            (control: AbstractControl) => {
                if (!this.form) return {};

                this.pwStrength = zxcvbn(control.value);
                setTimeout(() => this.form.get('confirmPassword').updateValueAndValidity(), 0);
            },
        ]),
        confirmPassword: new FormControl('admin1', [
            Validators.required,
            Validators.minLength(6),
            Validators.maxLength(100),
            (control: AbstractControl) => {
                if (!this.form) return {};

                const errors = {};
                if (control.value !== this.form.get('password').value) errors['mismatch'] = true;
                return errors;
            },
        ]),
    });

    @Output() completed = new EventEmitter<boolean>();

    constructor(public events: EventService, private userService: UserService, private authService: AuthService, private loadingCtrl: LoadingController) {}

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
        const regResp = await this.userService.register(this.form.value.name, this.form.value.email, this.form.value.username, this.form.value.password);

        // Stop if response is an exception
        if (regResp instanceof CustomError) {
            loading.dismiss();
            this.regError = regResp;
            return;
        }

        // Login user
        const loginResp = await this.authService.login(regResp.email, this.form.value.password);

        // Hide loading
        loading.dismiss();

        // Stop if response is an exception
        if (loginResp instanceof CustomError) {
            this.loginError = loginResp;
            return;
        }

        this.completed.emit();
    }
}
