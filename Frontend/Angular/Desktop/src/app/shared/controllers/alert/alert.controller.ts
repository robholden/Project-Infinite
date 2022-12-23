import { Injectable } from '@angular/core';

import { Trx } from '@shared/models';
import { AppColour } from '@shared/types';

import { ModalOptions } from '../modal';
import { ModalController } from '../modal/modal.controller';
import { AlertComponent, Button, DismissAction, Input } from './template/alert.component';

export interface SharedAlertOptions<T = {}> extends ModalOptions<T> {
    title?: string | Trx;
    message?: string | Trx;
    focusFirst?: boolean;
}

export interface AlertOptions extends SharedAlertOptions<DismissAction> {
    buttons?: Button[];
    inputs?: Input[];
}

export interface ConfirmOptions extends SharedAlertOptions {
    confirmBtn?: Button;
    cancelBtn?: Button;
}

@Injectable({
    providedIn: 'root',
})
export class AlertController {
    constructor(private modalCtrl: ModalController) {}

    async alert(title: string | Trx, message: string | Trx, closeText: string, colour?: AppColour) {
        await this.create({
            title,
            message,
            buttons: [
                {
                    text: closeText,
                    role: 'submit',
                    colour: colour || 'primary',
                    className: 'mx-a',
                },
            ],
        });
    }

    async confirm(options: ConfirmOptions): Promise<boolean> {
        const confirmBtn = options.confirmBtn || {
            text: 'Confirm',
            role: 'submit',
        };

        const cancelBtn = options.cancelBtn || {
            text: 'Cancel',
            role: 'cancel',
        };

        const result = await this.create({
            title: options.title,
            message: options.message,
            buttons: [
                {
                    text: cancelBtn.text,
                    role: 'cancel',
                    colour: cancelBtn.colour,
                    className: 'mr-a',
                },
                {
                    text: confirmBtn.text,
                    role: 'submit',
                    colour: confirmBtn.colour || 'primary',
                    className: 'ml-a',
                },
            ],
            focusFirst: options.focusFirst,
            dismissWhen: options.dismissWhen,
        });

        return result !== null;
    }

    create(options: AlertOptions) {
        const modal = this.modalCtrl.add(new Date().toString(), AlertComponent, {
            title: options.title,
            message: options.message,
            inputs: options.inputs,
            buttons: options.buttons,
            focusFirst: options.focusFirst,
        });

        return modal.present({ dismissWhen: options.dismissWhen, dimissAction: modal });
    }
}
