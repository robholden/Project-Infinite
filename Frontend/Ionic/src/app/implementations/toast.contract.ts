import { Inject, Injectable } from '@angular/core';

import { TranslateService } from '@ngx-translate/core';

import { ToastController } from '@ionic/angular';

import { trxTransform } from '@shared/functions';
import { INJ_TRANSLATION } from '@shared/injectors';
import { Toast } from '@shared/interfaces';
import { Trx } from '@shared/models';

@Injectable({
    providedIn: 'root',
})
export class ToastContract implements Toast {
    constructor(@Inject(INJ_TRANSLATION) private translate: TranslateService, private toastController: ToastController) {}

    async showError(message: string | Trx, duration?: number): Promise<void> {
        return await this.show(message, 'danger', duration);
    }

    async showMessage(message: string | Trx, duration?: number): Promise<void> {
        return await this.show(message, 'primary', duration);
    }

    private async show(message: string | Trx, colour: 'primary' | 'danger', duration?: number): Promise<void> {
        // Dismiss all
        const overlay = await this.toastController.getTop();
        if (overlay) await this.toastController.dismiss();

        // Show toast
        const toast = await this.toastController.create({
            message: message instanceof Trx ? trxTransform(this.translate, message) : message,
            color: colour,
            duration: duration || 3000,
        });
        await toast.present();
    }
}
