import { Injectable } from '@angular/core';

import { wait } from '@shared/functions';
import { CustomError, Trx } from '@shared/models';

import { ControllersService } from '../controllers.service';
import { ILoadingComponent, LoadingComponent } from './template/loading.component';

export interface LoadingBtn {
    id: string;
    element: HTMLElement;
    present: () => void;
    dismiss: () => void;
}

@Injectable({
    providedIn: 'root',
})
export class LoadingController {
    constructor(private ctrl: ControllersService) {}

    add(message: Trx | string, opts?: { animate?: boolean; opaque?: boolean }): ILoadingComponent {
        opts = opts || { animate: true, opaque: false };

        return this.ctrl.create(LoadingComponent, (instance) => {
            instance.message = message;
            instance.animate = opts.animate !== false;
            instance.opaque = opts.opaque === true;
        });
    }

    addBtn(id: string): LoadingBtn {
        const element = document.getElementById(id);
        return {
            id,
            element,
            present: () => this.setButtonState(element, true),
            dismiss: () => this.setButtonState(element, false),
        };
    }

    async addBtnWithApi<T>(id: string, prom: Promise<T | CustomError>) {
        const element = document.getElementById(id);

        this.setButtonState(element, true);

        const resp = await prom;
        this.setButtonState(element, false, resp instanceof CustomError);

        return resp;
    }

    private async setButtonState(btn: HTMLElement, load: boolean, error: boolean = null) {
        if (!btn) return;
        btn.classList.add('pi-loading');

        if (load) {
            const content = btn.innerHTML;
            const loadingContent = '<span><i class="fas fa-spinner-third fa-spin icon" position="middle"></i></span>';

            if (!btn.classList.contains('pi-loading')) btn.classList.add('pi-loading');

            btn.setAttribute('data-content', content);

            if (btn.hasAttribute('disabled')) btn.setAttribute('data-disabled', btn.getAttribute('disabled'));

            btn.innerHTML = loadingContent;
            btn.setAttribute('disabled', 'disabled');
        } else {
            const html = btn.getAttribute('data-content');
            if (btn.hasAttribute('data-disabled')) {
                btn.setAttribute('disabled', btn.getAttribute('data-disabled'));
                btn.removeAttribute('data-disabled');
            } else btn.removeAttribute('disabled');

            btn.removeAttribute('data-content');

            if (error !== null) {
                const bg = btn.getAttribute('background');
                btn.setAttribute('background', !error ? 'success' : 'danger');
                btn.innerHTML = `<span><i class="fas fa-${!error ? 'check' : 'times'} icon" position="middle"></i></span>`;
                await wait(50);

                btn.classList.add('waiting');

                await wait(250);
                btn.classList.remove('waiting');

                await wait(1000);

                if (bg) btn.setAttribute('background', bg);
                else btn.removeAttribute('background');

                btn.innerHTML = html;
                btn.classList.remove('fade-out');
            } else {
                btn.innerHTML = html;
            }

            btn.classList.remove('pi-loading');
        }
    }
}
