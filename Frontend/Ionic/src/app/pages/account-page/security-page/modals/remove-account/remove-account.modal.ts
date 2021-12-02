import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { removeTouchId } from '@app/functions/touch-id.fn';

import { LoadingController, ModalController, ToastController } from '@ionic/angular';
import { Storage } from '@ionic/storage';

import { CustomError } from '@shared/models';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'sc-remove-account',
    templateUrl: './remove-account.modal.html',
    styleUrls: ['./remove-account.modal.scss'],
})
export class RemoveAccountModal implements OnInit {
    form: FormGroup = this.fb.group({
        password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
        confirmed: [false, [Validators.requiredTrue]],
    });

    get password() {
        return this.form.get('password');
    }

    constructor(
        private fb: FormBuilder,
        private authState: AuthState,
        private modalCtrl: ModalController,
        private ionStorage: Storage,
        private userSevice: UserService,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {
        if (this.authState.provider) this.password.disable();
    }

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss();
    }

    async confirm() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Deleting Account',
            spinner: 'crescent',
        });
        await loading.present();

        // Talk to our api and log them in
        const resp = await this.userSevice.deleteAccount(this.password.value);
        await loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return;
        }

        // Remove touch id
        await removeTouchId(this.ionStorage);

        // Password reset successfully
        await this.dismiss();
        const toast = await this.toastCtrl.create({
            message: 'Your account have been removed successfully',
            color: 'success',
            duration: 2500,
        });
        await toast.present();
    }
}
