import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { TwoFactorType } from '@shared/enums';
import { CustomError, ErrorCode, RecoveryCode } from '@shared/models';
import { AuthService, TwoFactorResponse } from '@shared/services/identity';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';

import qrcode from 'qrcode';

@Component({
    selector: 'pi-setup-two-factor',
    templateUrl: './setup-two-factor.modal.html',
    styleUrls: ['./setup-two-factor.modal.scss'],
})
export class SetupTwoFactorModal extends ModalComponent<any> implements OnInit, OnDestroy {
    @Input() type: TwoFactorType;
    @Input() data: TwoFactorResponse;
    @Input() email: string;

    recoveryCodes: RecoveryCode[];
    qrcode: string;
    form: FormGroup = new FormGroup({
        code: new FormControl('', [Validators.required, Validators.pattern(/^[0-9]+$/), Validators.minLength(6), Validators.maxLength(6)]),
    });

    get TwoFactorType() {
        return TwoFactorType;
    }

    constructor(private authService: AuthService, private loadingCtrl: LoadingController, private alertCtrl: AlertController) {
        super();
    }

    ngOnInit() {
        if (this.type === TwoFactorType.App) this.generateQR();
        this.preventClosing(true);
    }

    ngOnDestroy() {
        this.preventClosing(false);
    }

    async generateQR() {
        if (!this.data || !this.data.secret) return;

        const secret = this.data.secret;
        this.data.secret = this.data.secret
            .replace(/[^\dA-Z]/g, '')
            .replace(/(.{4})/g, '$1 ')
            .trim();

        this.qrcode = await qrcode.toDataURL(
            `otpauth://totp/Snow%20Capture:${this.email}?secret=${secret}&issuer=Snow%20Capture&algorithm=${this.data.mode}&digits=${this.data.size}&period=${this.data.step}`
        );
    }

    // Completes setup of 2FA
    //
    async complete() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('complete-btn');
        loading.present();

        // Call api
        const resp = await this.authService.enable2FA(this.type, this.form.value.code);

        // Hide loading
        loading.dismiss();
        this.options.canDismiss = true;

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.ProvidedCodeInvalid) this.form.patchValue({ code: '' });
            return;
        }

        // Show user their recovery codes
        this.recoveryCodes = resp;
    }

    async closeWarn() {
        if (this.recoveryCodes) {
            const confirmed = await this.alertCtrl.confirm({
                title: 'Save your recovery codes',
                message: 'Ensure you have saved your recovery codes in a safe place. You will not be able to view these codes again!',
                confirmBtn: {
                    text: 'Yes, I have saved them',
                    colour: 'danger',
                },
                focusFirst: true,
            });
            if (!confirmed) return;
        }

        this.dismiss();
    }
}
