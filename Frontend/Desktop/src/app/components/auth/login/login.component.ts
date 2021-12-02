import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ToastController } from '@app/shared/controllers/toast';

import { TwoFactorType } from '@shared/enums';
import { CustomError, ErrorCode, Trx } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, PasswordService } from '@shared/services/identity';

@Component({
    selector: 'sc-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
    form: FormGroup;
    error: Trx;

    @Output() completed = new EventEmitter<boolean>();

    constructor(
        public events: EventService,
        private fb: FormBuilder,
        private authService: AuthService,
        private passwordService: PasswordService,
        private alertCtrl: AlertController,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController
    ) {
        this.setForm();
    }

    ngOnInit() {
        setTimeout(() => document.getElementById('username').focus(), 0);
    }

    async login() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('login-btn');
        loading.present();
        this.form.disable();
        this.error = null;

        // Talk to our api and log them in
        const resp = await this.authService.login(this.form.get('username').value, this.form.get('password').value);

        // Hide loading
        this.form.enable();
        loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.IncorrectUsernameOrPassword) {
                this.form.patchValue({ password: '' });
                document.getElementById('password').focus();
            }
            this.error = resp;
            return;
        }

        // Check for 2FA
        if (!resp.user && resp.twoFactorRequired !== TwoFactorType.Unset) {
            this.completed.emit(false);
            return;
        }

        this.completed.emit(true);
    }

    async forgotPassword() {
        // Show prompt
        const inputs = await this.alertCtrl.create({
            title: 'Forgotten Password',
            message: 'Enter your email address for a password reset link',
            inputs: [
                {
                    name: 'email',
                    type: 'email',
                    mode: 'email',
                    validators: [Validators.required, Validators.email, Validators.maxLength(255)],
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Submit',
                    role: 'submit',
                    colour: 'primary',
                    className: 'primary',
                },
            ],
        });
        if (!inputs) return;

        // Tell ui we're loading
        const loading = this.loadingCtrl.add('Sending request');
        loading.present();

        // Call service
        const resp = await this.passwordService.forgot(inputs.email);

        // Hide loading
        await loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return;
        }

        const toast = this.toastCtrl.add('An email has been sent on how to reset your password', 'success');
        toast.present(5000);
    }

    private setForm() {
        this.form = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(255)]],
            password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
            remember: [false],
        });
    }
}
