import { Component, HostBinding, HostListener, inject, Input, OnInit } from '@angular/core';

import { BehaviorSubject } from 'rxjs';

import { DesktopStore } from '@app/storage/desktop.store';

import { ControllersService, IControllerComponent } from '../controllers.service';

export interface IModalComponent<T> extends IControllerComponent {
    present(options?: ModalOptions): Promise<T>;
    dismiss(): Promise<void>;
}

export interface ModalOptions<T = {}> {
    canDismiss?: boolean;
    dismissOnEscape?: boolean;
    dimissAction?: T;
    dismissWhen?: (result: any, dismissAction?: T) => Promise<boolean>;
}

@Component({
    selector: 'pi-modal-component',
    template: '',
})
export class ModalComponent<T> implements IModalComponent<T>, OnInit {
    id: string;
    options: ModalOptions = {
        canDismiss: true,
        dismissOnEscape: true,
    };
    dismissing: boolean;

    protected desktopStore = inject(DesktopStore);
    protected ctrl = inject(ControllersService);
    protected result = new BehaviorSubject<T>(null);

    @HostBinding('attr.class') className = 'modal-content';
    @Input() text: string;

    ngOnInit() {}

    present(options?: ModalOptions) {
        const wrapper = document.createElement('div');
        wrapper.classList.add('modal-wrapper');

        const container = document.createElement('div');
        container.classList.add('modal-container');

        const overlay = document.createElement('div');
        overlay.classList.add('modal-overlay');

        wrapper.appendChild(overlay);
        wrapper.appendChild(container);

        this.ctrl.show(this.id, container, (el) => el.classList.add('loaded'), wrapper);

        if (options) Object.keys(options).forEach((key) => (this.options[key] = options[key]));

        let init = false;
        return new Promise<T>((res) => {
            this.result.asObservable().subscribe(
                (value: T) => {
                    if (init) {
                        this.dismiss();
                        res(value);
                    }

                    init = true;
                },
                () => res(this.result.value),
                () => res(this.result.value)
            );
        });
    }

    async dismiss() {
        if (this.options.canDismiss === false) return;

        if (this.options.dismissWhen) {
            this.dismissing = true;
            const canDismiss = await this.options.dismissWhen(this.result.value, this.options.dimissAction);
            this.dismissing = false;
            if (!canDismiss) return;
        }

        this.ctrl.destroy(this.id);
        this.result.complete();
    }

    complete(result: T) {
        this.result.next(result);
    }

    @HostListener('document:keydown.escape')
    onEscape(): void {
        if (this.options?.dismissOnEscape) this.dismiss();
    }

    preventClosing(prevent: boolean) {
        if (prevent) this.desktopStore.setCanReload({ fn: () => !prevent, id: this.id });
        else this.desktopStore.unsetCanReload(this.id);
        this.options.canDismiss = !prevent;
    }
}
