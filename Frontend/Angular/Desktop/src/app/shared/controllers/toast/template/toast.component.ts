import { Component, HostBinding, Input, OnInit } from '@angular/core';

import { BehaviorSubject } from 'rxjs';

import { waitThen } from '@shared/functions';
import { Trx } from '@shared/models';
import { AppColour } from '@shared/types';

import { ControllersService, IControllerComponent } from '../../controllers.service';

export interface IToastComponent extends IControllerComponent {
    colour: AppColour;
    message: Trx | string;

    present(duration?: number): Promise<void>;
    dismiss(): void;
}

@Component({
    selector: 'sc-toast-component',
    templateUrl: './toast.component.html',
    styleUrls: ['./toast.component.scss'],
})
export class ToastComponent implements ToastComponent, OnInit {
    id: string;
    duration: number = null;

    @HostBinding('class.preloaded') preloaded = true;
    @HostBinding('class.dismissed') dismissed = false;

    @Input() message: Trx | string;
    @HostBinding('attr.background') @Input() colour: AppColour;

    private _result = new BehaviorSubject<boolean>(false);

    constructor(private ctrl: ControllersService) {}

    ngOnInit() {}

    present(duration?: number) {
        this.ctrl.show(this.id);

        return new Promise<void>(async (res) => {
            this._result.subscribe((value) => {
                if (!value) return;

                this.ctrl.destroy(this.id);
                res();
            });

            if (duration) setTimeout(() => this.dismiss(), duration);

            setTimeout(() => {
                this.duration = duration || 0;
                this.preloaded = false;
            }, 10);
        });
    }

    async dismiss() {
        this.dismissed = true;
        await waitThen(10, () => this._result.next(true));
    }
}
