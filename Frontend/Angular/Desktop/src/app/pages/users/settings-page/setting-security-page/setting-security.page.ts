import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { combineLatest, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ExternalProvider, TwoFactorType } from '@shared/enums';
import { zxcvbn } from '@shared/functions';
import { CustomError, Trx, User } from '@shared/models';
import { AuthService, PasswordService, UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

import { AlertController, DismissAction, Input } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ModalController } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

import { SetupTwoFactorModal } from './setup-two-factor-modal/setup-two-factor.modal';

@Component({
    selector: 'pi-setting-security',
    templateUrl: './setting-security.page.html',
    styleUrls: ['./setting-security.page.scss'],
})
export class SettingSecurityPage implements OnInit, OnDestroy {
    private destroy$ = new Subject();

    user: User;
    provider: ExternalProvider;

    get usingAny(): boolean {
        return this.user?.twoFactorEnabled && this.user?.twoFactorType !== TwoFactorType.Unset;
    }

    get usingApp(): boolean {
        return this.user?.twoFactorEnabled && this.user?.twoFactorType === TwoFactorType.App;
    }

    get usingEmail(): boolean {
        return this.user?.twoFactorEnabled && this.user?.twoFactorType === TwoFactorType.Email;
    }

    get usingSms(): boolean {
        return this.user?.twoFactorEnabled && this.user?.twoFactorType === TwoFactorType.Sms;
    }

    constructor(
        public authState: AuthState,
        private router: Router,
        private authService: AuthService,
        private passwordService: PasswordService,
        private userService: UserService,
        private modalCtrl: ModalController,
        private alertCtrl: AlertController,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController
    ) {}

    ngOnInit() {
        combineLatest([this.authState.observe('user'), this.authState.observe('provider')])
            .pipe(takeUntil(this.destroy$))
            .subscribe(([user, provider]) => {
                this.user = user;
                this.provider = provider;
            });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
    }

    async changePassword() {
        // Prompt for password when update email or mobile
        const result = await this.alertCtrl.create({
            title: 'Change Password',
            inputs: [
                {
                    label: 'Current Password',
                    name: 'password',
                    type: 'password',
                    autocomplete: 'new-password',
                    validators: [Validators.required, Validators.minLength(6), Validators.maxLength(100)],
                },
                {
                    label: 'New Password',
                    name: 'newPassword',
                    type: 'password',
                    autocomplete: 'new-password',
                    validators: [
                        Validators.required,
                        Validators.minLength(6),
                        Validators.maxLength(100),
                        (control: AbstractControl) => {
                            if (!control.parent) return;
                            setTimeout(() => control.parent.get('confirmPassword').updateValueAndValidity(), 0);
                            return {};
                        },
                    ],
                    displayMessages: (value: string) => {
                        const strength = zxcvbn(value);
                        return strength ? [{ colour: strength.colour, text: strength.message }] : [];
                    },
                },
                {
                    label: 'Confirm Password',
                    name: 'confirmPassword',
                    type: 'password',
                    autocomplete: 'new-password',
                    errorMap: { mismatch: new Trx('form_errors.mismatch', { name: 'Passwords' }) },
                    onlyUseErrorMap: true,
                    validators: [
                        Validators.required,
                        Validators.minLength(6),
                        Validators.maxLength(100),
                        (control: AbstractControl) => {
                            if (!control.parent) return;
                            return control.value !== control.parent.get('newPassword').value ? { mismatch: true } : {};
                        },
                    ],
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Change Password',
                    role: 'submit',
                    colour: 'primary',
                    className: 'ml-a',
                },
            ],
        });

        if (!result) return;
        const password: string = result.password;
        const newPassword: string = result.newPassword;

        // Show loading
        const loading = this.loadingCtrl.addBtn('password-btn');
        loading.present();

        // Go to api
        const resp = await this.passwordService.change(password, newPassword);

        // Hide loading
        loading.dismiss();

        // Show success?
        if (!(resp instanceof CustomError)) {
            const toast = this.toastCtrl.add('You have successfully updated your password', 'success');
            toast.present(5000);
        }
    }

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
        const inputs: Input[] = [
            {
                name: 'password',
                type: 'password',
                autocomplete: 'new-password',
                label: 'Enter your password to begin the setup',
                validators: [Validators.required, Validators.minLength(6)],
            },
        ];

        if (type === TwoFactorType.Sms) {
            inputs.unshift({
                name: 'mobile',
                type: 'text',
                autocomplete: 'mobile',
                placeholder: '+44',
                mode: 'tel',
                label: 'Enter the mobile number you want to use for receiving your two-factor passcode',
                validators: [Validators.required, Validators.minLength(8), Validators.maxLength(15)],
            });
        }

        await this.alertCtrl.create({
            title: 'Two-Factor Authentication Setup',
            inputs,
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Setup',
                    role: 'submit',
                    colour: 'primary',
                    className: 'ml-a',
                },
            ],
            dismissWhen: async (result, di?: DismissAction) => {
                if (!result || !result.password) return true;
                const success = await this.setup(type, result.password, result.mobile);
                if (!success) {
                    di?.clearInput('password');
                    return false;
                }

                return true;
            },
        });
    }

    private async setup(type: TwoFactorType, password: string, mobile: string) {
        // Tell ui we're loading
        const btnLoading = this.loadingCtrl.addBtn(`enable-${type}-btn`);

        btnLoading.present();

        // Call api
        const resp = await this.authService.setup2FA(type, { password }, mobile);

        // Hide loading
        btnLoading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return false;
        }

        const modal = this.modalCtrl.add('2fa', SetupTwoFactorModal, { type, data: resp, email: this.user?.email });
        modal.present();

        return true;
    }

    async remove() {
        await this.disable2FA();
    }

    private async disable2FA() {
        const result = await this.alertCtrl.create({
            title: 'Remove Two-Factor Authentication',
            message: 'Your password is required to make this change',
            inputs: [
                {
                    name: 'password',
                    type: 'password',
                    placeholder: 'Your Password',
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
                    text: 'Remove',
                    role: 'submit',
                    colour: 'danger',
                    className: 'ml-a',
                },
            ],
        });

        if (!result || !result.password) return;

        // Tell ui we're loading

        const loading = this.loadingCtrl.addBtn('remove-btn');
        loading.present();

        const resp = await this.authService.disable2FA(result.password);

        // Hide loading
        loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        const toast = this.toastCtrl.add('Two-factor authentication has been removed', 'success');
        toast.present(5000);
    }

    async deleteAccount() {
        // Define inputs
        const inputs: Input[] = [
            {
                name: 'confirm',
                type: 'checkbox',
                label: 'Check this box to confirm you have read the warning above',
                value: false,
                validators: [Validators.requiredTrue],
            },
        ];

        // Only prompt input for internal logins
        if (!this.authState.snapshot('provider')) {
            inputs.push({
                name: 'password',
                type: 'password',
                label: 'Enter your password to continue',
                autocomplete: 'new-password',
                validators: [Validators.required, Validators.minLength(6)],
            });
        }

        // Prompt for password when update email or mobile
        const result = await this.alertCtrl.create({
            title: 'Delete Account',
            message: `<p class="colour-danger">When we delete your account all data is permanently deleted immediately and cannot be recovered</p>`,
            inputs,
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Permanently Delete Account',
                    role: 'submit',
                    colour: 'danger',
                    className: 'ml-a',
                },
            ],
        });

        if (!result) return;
        const password: string = result.password;

        // Show loading
        const loading = this.loadingCtrl.add('Deleting Account');
        loading.present();

        // Go to api
        const resp = await this.userService.deleteAccount(password);

        // Hide loading
        await loading.dismiss();

        // Stop on error
        if (resp instanceof CustomError) {
            return;
        }

        // Account removed
        const toast = this.toastCtrl.add('Your account has been removed successfully', 'success');
        toast.present(5000);
        await this.router.navigateByUrl('/');
    }
}
