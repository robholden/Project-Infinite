import { Component, OnInit } from '@angular/core';

import { ModalController } from '@ionic/angular';

@Component({
    selector: 'pi-terms-and-conditions',
    templateUrl: './terms-and-conditions.modal.html',
    styleUrls: ['./terms-and-conditions.modal.scss'],
})
export class TermsAndConditionsModal implements OnInit {
    constructor(private modalCtrl: ModalController) {}

    ngOnInit() {}

    async dismiss() {
        await this.modalCtrl.dismiss('terms');
    }
}
