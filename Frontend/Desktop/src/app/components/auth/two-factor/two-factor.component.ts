import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ToastController } from '@app/shared/controllers/toast';
import { DesktopStore } from '@app/storage/desktop.store';

import { TwoFactorType } from '@shared/enums';
import { CustomError, ErrorCode } from '@shared/models';
import { AuthService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'sc-two-factor',
    templateUrl: './two-factor.component.html',
    styleUrls: ['./two-factor.component.scss'],
})
export class TwoFactorComponent implements OnInit {
    ref: string;
    form: FormGroup;

    @Output() completed = new EventEmitter<boolean>();

    get TwoFactorType() {
        return TwoFactorType;
    }

    constructor(
        public authState: AuthState,
        private fb: FormBuilder,
        private authService: AuthService,
        private loadingCtrl: LoadingController,
        private alertCtrl: AlertController,
        private toastCtrl: ToastController,
        private desktopStore: DesktopStore
    ) {
        this.setForm();
    }

    ngOnInit() {
        this.desktopStore.preventRefresh(true);
        setTimeout(() => {
            const el = document.getElementById('code');
            if (el) el.focus();
        }, 0);
    }

    dismiss() {
        this.completed.emit();
        this.desktopStore.preventRefresh(false);
    }

    async verify() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('continue-btn');
        loading.present();

        // Talk to our api and log them in
        const resp = await this.authService.verify(this.form.get('code').value, this.form.get('doNotAskAgain').value);

        // Hide loading
        loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.ProvidedCodeInvalid) this.form.patchValue({ code: '' });
            return;
        }

        this.dismiss();
    }

    // Resends the verification code
    //
    async resend() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.add('Resending Code');
        loading.present();

        // Go to api
        const resp = await this.authService.resend();

        // Hide loading
        await loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return;
        }

        // Resend successful
        const toast = this.toastCtrl.add('A code has been successfully re-sent', 'success');
        toast.present(5000);
    }

    // Triggers prompt recovering an account
    //
    async recover() {
        // Show prompt
        const inputs = await this.alertCtrl.create({
            title: 'Recover Account',
            message: 'Enter one of your recovery codes',
            inputs: [
                {
                    name: 'code',
                    type: 'text',
                    validators: [Validators.required, Validators.minLength(10)],
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Recover Account',
                    role: 'submit',
                    colour: 'danger',
                },
            ],
        });
        if (!inputs) return;

        // Quit if email is null
        if (!inputs.code) return;

        // Tell ui we're loading
        const loading = this.loadingCtrl.add('Recovering Account');
        loading.present();

        // Call service
        const resp = await this.authService.recover(inputs.code);

        // Hide loading
        await loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return;
        }

        const toast = this.toastCtrl.add('Account recovered. Two-factor authentication has been removed.', 'warning');
        toast.present(5000);

        this.dismiss();
    }

    // Sets the form data
    //
    private setForm() {
        this.form = this.fb.group({
            code: ['', [Validators.required, Validators.pattern(/^[0-9]+$/), Validators.minLength(6), Validators.maxLength(6)]],
            doNotAskAgain: [false],
        });
    }
}
