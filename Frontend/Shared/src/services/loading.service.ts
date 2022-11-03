import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class LoadingService {
    private _queueSize = 0;

    get visible() {
        return this._queueSize > 0;
    }

    constructor() {}

    show() {
        this._queueSize++;
    }

    hide() {
        if (this._queueSize > 0) this._queueSize--;
    }
}
