import { Injectable } from '@angular/core';

import { Observable, Subject } from 'rxjs';

import { CustomStore } from './store';

export type AppSchema = {
    nightMode: boolean;
};

@Injectable({
    providedIn: 'root',
})
export class AppState extends CustomStore<AppSchema> {
    private _updateTrx = new Subject<void>();
    language$: Observable<void> = this._updateTrx.asObservable();

    constructor() {
        super({
            nightMode: {
                store: ['local'],
            },
        });

        this.observe('nightMode').subscribe((night) => this.applyTheme(night));
    }

    async toggleNightMode() {
        this.set('nightMode', !this.snapshot('nightMode'));
    }

    updateTrx() {
        this._updateTrx.next();
    }

    private applyTheme(night_mode: boolean) {
        const body = document.getElementsByTagName('body')[0];
        if (!night_mode) body.classList.remove('night-theme');
        else body.classList.add('night-theme');
    }
}
