import { Component, OnInit } from '@angular/core';

import { ModalController } from '@ionic/angular';

@Component({
    selector: 'pi-privacy-policy',
    templateUrl: './privacy-policy.modal.html',
    styleUrls: ['./privacy-policy.modal.scss'],
})
export class PrivacyPolicyModal implements OnInit {
    constructor(private modalCtrl: ModalController) {}

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss('privacy');
    }
}
