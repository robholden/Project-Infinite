import { Component, OnInit } from '@angular/core';

import { ModalController, ToastController } from '@ionic/angular';

@Component({
    selector: 'pi-update-username',
    templateUrl: './update-username.modal.html',
    styleUrls: ['./update-username.modal.scss'],
})
export class UpdateUsernameModal implements OnInit {
    constructor(private modalCtrl: ModalController, private toastCtrl: ToastController) {}

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss(true);
    }

    async saved() {
        const toast = await this.toastCtrl.create({ message: 'Your username has been updated', color: 'success', duration: 2500 });
        toast.present();

        await this.dismiss();
    }
}
