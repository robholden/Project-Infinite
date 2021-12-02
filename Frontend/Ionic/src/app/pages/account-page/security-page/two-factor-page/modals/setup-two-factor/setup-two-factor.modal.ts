import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { LoadingController, ModalController, ToastController } from '@ionic/angular';

import { TwoFactorType } from '@shared/enums';
import { CustomError, ErrorCode, RecoveryCode } from '@shared/models';
import { AuthService, TwoFactorResponse } from '@shared/services/identity';

import { Clipboard } from '@capacitor/clipboard';
import qrcode from 'qrcode';

@Component({
    selector: 'sc-setup-two-factor',
    templateUrl: './setup-two-factor.modal.html',
    styleUrls: ['./setup-two-factor.modal.scss'],
})
export class SetupTwoFactorModal implements OnInit {
    @Input() type: TwoFactorType;
    @Input() data: TwoFactorResponse;
    @Input() email: string;

    TwoFactorType = TwoFactorType;
    recoveryCodes: RecoveryCode[];
    qrcode: string;
    form: FormGroup = this.fb.group({
        code: ['', [Validators.required, Validators.pattern(/^[0-9]+$/), Validators.minLength(6), Validators.maxLength(6)]],
    });

    get code() {
        return this.form.get('code');
    }

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private modalCtrl: ModalController,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {}

    ngOnInit() {
        this.generateQR();
    }

    async dismiss() {
        await this.modalCtrl.dismiss(true);
    }

    async generateQR() {
        if (!this.data?.secret) return;

        const secret = this.data.secret;
        this.data.secret = this.data.secret
            .replace(/[^\dA-Z]/g, '')
            .replace(/(.{4})/g, '$1 ')
            .trim();

        this.qrcode = await qrcode.toDataURL(
            `otpauth://totp/Snow Capture:${this.email}?secret=${secret}&issuer=Snow Capture&algorithm=${this.data.mode}&digits=${this.data.size}&period=${this.data.step}`
        );
    }

    async complete() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Completing Setup',
            spinner: 'crescent',
        });
        await loading.present();

        // Call api
        const resp = await this.authService.enable2FA(this.type, this.code.value);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            if (resp.code === ErrorCode.ProvidedCodeInvalid) this.form.patchValue({ code: '' });
            await loading.dismiss();
            return;
        }

        // Show user their recovery codes
        this.recoveryCodes = resp;
        await loading.dismiss();
    }

    async copyToClipboard(value: string) {
        await Clipboard.write({ string: value });

        const toast = await this.toastCtrl.create({ message: 'Code copied to clipboard', duration: 1000 });
        toast.present();
    }
}
