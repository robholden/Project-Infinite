import { Component, OnInit } from '@angular/core';

import { ModalController, ToastController } from '@ionic/angular';

@Component({
    selector: 'sc-update-name',
    templateUrl: './update-name.modal.html',
    styleUrls: ['./update-name.modal.scss'],
})
export class UpdateNameModal implements OnInit {
    constructor(private modalCtrl: ModalController, private toastCtrl: ToastController) {}

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss(true);
    }

    async saved() {
        const toast = await this.toastCtrl.create({ message: 'Your name has been updated', color: 'success', duration: 2500 });
        toast.present();

        await this.dismiss();
    }
}
