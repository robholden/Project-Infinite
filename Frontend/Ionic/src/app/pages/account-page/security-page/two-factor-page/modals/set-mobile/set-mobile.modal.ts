import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ModalController } from '@ionic/angular';

@Component({
    selector: 'sc-set-mobile',
    templateUrl: './set-mobile.modal.html',
    styleUrls: ['./set-mobile.modal.scss'],
})
export class SetMobileModal implements OnInit {
    form: FormGroup = this.fb.group({
        mobile: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(15)]],
    });

    get mobile() {
        return this.form.get('mobile');
    }

    constructor(private fb: FormBuilder, private modalCtrl: ModalController) {}

    ngOnInit() {}

    async dismiss(mobile?: string) {
        await this.modalCtrl.dismiss(mobile, null, 'set-mobile');
    }

    async set() {
        await this.dismiss(this.mobile.value);
    }
}
