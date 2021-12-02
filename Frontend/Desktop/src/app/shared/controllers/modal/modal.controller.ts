import { Injectable, Type } from '@angular/core';

import { ControllersService } from '../controllers.service';
import { ModalComponent } from './modal.component';

@Injectable()
export class ModalController {
    private ids: string[] = [];

    constructor(private ctrl: ControllersService) {}

    dismiss(id?: string) {
        if (id) {
            this.ctrl.destroy(id);
            this.ids = this.ids.filter((v) => v !== id);
            return;
        }

        this.ids.forEach((i) => this.ctrl.destroy(i));
        this.ids = [];
    }

    add<T>(id: string, component: Type<ModalComponent<T>>, props?: any): ModalComponent<T> {
        if (!this.ids.includes(id)) this.ids.push(id);

        return this.ctrl.create(
            component,
            (instance) => {
                if (props) Object.keys(props).forEach((prop) => (instance[prop] = props[prop]));
            },
            id
        );
    }
}
