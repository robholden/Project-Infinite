import { Injectable } from '@angular/core';

import { Toast } from '@shared/interfaces';
import { Trx } from '@shared/models';

import { ToastController } from '../controllers/toast/toast.controller';

@Injectable({
    providedIn: 'root',
})
export class ToastContract implements Toast {
    constructor(private toastCtrl: ToastController) {}

    async showError(message: Trx | string, duration?: number): Promise<void> {
        const toast = this.toastCtrl.add(message, 'danger');
        await toast.present(duration || 5000);
    }

    async showMessage(message: Trx | string, duration?: number): Promise<void> {
        const toast = this.toastCtrl.add(message, 'primary');
        await toast.present(duration || 5000);
    }
}
