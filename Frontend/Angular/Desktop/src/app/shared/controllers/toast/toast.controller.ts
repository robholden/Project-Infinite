import { Injectable } from '@angular/core';

import { Trx } from '@shared/models';
import { AppColour } from '@shared/types';

import { ControllersService } from '../controllers.service';
import { IToastComponent, ToastComponent } from './template/toast.component';

@Injectable({
    providedIn: 'root',
})
export class ToastController {
    constructor(private ctrl: ControllersService) {}

    add(message: Trx | string, colour: AppColour = 'primary'): IToastComponent {
        return this.ctrl.create(ToastComponent, (instance) => {
            instance.message = message;
            instance.colour = colour;
        });
    }
}
