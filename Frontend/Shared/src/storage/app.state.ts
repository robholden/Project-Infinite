import { Injectable } from '@angular/core';

import { BehaviorSubject, Observable, Subject } from 'rxjs';

import { preferNightMode } from '@shared/functions';
import { StorageService } from '@shared/services';

export class AppStore {
    night_mode: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class AppState {
    private _loaded = new BehaviorSubject<boolean>(false);
    private _updateTrx = new Subject<void>();

    loaded$: Observable<boolean> = this._loaded.asObservable();
    nightMode$: Observable<boolean>;
    language$: Observable<void> = this._updateTrx.asObservable();

    constructor(public storage: StorageService<AppStore>) {}

    async load() {
        await this.storage.load<boolean>('night_mode', null, preferNightMode());

        this.nightMode$ = this.storage.watch('night_mode');
        this.nightMode$.subscribe((night_mode) => this.applyTheme(night_mode));

        this._loaded.next(true);
    }

    async toggleNightMode() {
        this.storage.set('night_mode', !this.storage.get<boolean>('night_mode'));
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
