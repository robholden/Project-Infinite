import { Component, HostBinding, Input, OnInit } from '@angular/core';

import { BehaviorSubject } from 'rxjs';

import { Trx } from '@shared/models';

import { ControllersService, IControllerComponent } from '../../controllers.service';

export interface ILoadingComponent extends IControllerComponent {
    message: Trx | string;

    present(): void;
    dismiss(): Promise<void>;
}

@Component({
    selector: 'pi-loading-component',
    templateUrl: './loading.component.html',
    styleUrls: ['./loading.component.scss'],
})
export class LoadingComponent implements LoadingComponent, OnInit {
    id: string;

    @Input() message: Trx | string;
    @HostBinding('class.opaque') @Input() opaque: boolean;
    @HostBinding('class.animate') @Input() animate: boolean;

    private dismissed = false;
    private timeoutReached = false;

    private completed = new BehaviorSubject(false);

    constructor(private ctrl: ControllersService) {}

    ngOnInit() {
        this.timeoutReached = !this.animate;
    }

    present() {
        this.ctrl.show(this.id, null, (el) => el.classList.add('loaded'));

        setTimeout(() => {
            this.timeoutReached = true;
            if (this.dismissed) this.close();
        }, 1000);
    }

    private close() {
        if (this.timeoutReached) {
            this.ctrl.destroy(this.id);
            this.completed.next(true);
        }
        this.dismissed = true;
    }

    async dismiss() {
        return new Promise<void>((res) => {
            this.close();

            this.completed.subscribe((value) => {
                if (!value) return;
                res();
            });
        });
    }
}
