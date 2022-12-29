import { Component, OnInit } from '@angular/core';

import { LoadingController, ModalController, ToastController } from '@ionic/angular';

import { CustomError } from '@shared/models';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'pi-update-email',
    templateUrl: './update-email.modal.html',
    styleUrls: ['./update-email.modal.scss'],
})
export class UpdateEmailModal implements OnInit {
    sent: boolean;

    constructor(
        public authState: AuthState,
        private userService: UserService,
        private modalCtrl: ModalController,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController
    ) {}

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss(true);
    }

    async saved() {
        const toast = await this.toastCtrl.create({ message: 'Your email has been updated', color: 'success', duration: 2500 });
        toast.present();

        await this.dismiss();
    }

    async resend() {
        if (this.sent) return;

        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({ message: 'Resending Email', spinner: 'crescent' });
        await loading.present();

        // Talk to api
        const resp = await this.userService.resendConfirmEmail();
        await loading.dismiss();

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            return;
        }

        // Email sent!
        this.sent = true;
    }
}
