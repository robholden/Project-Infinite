import { Injectable } from '@angular/core';

import { Storage as IonicStorage } from '@ionic/storage';

import { Storage } from '@shared/interfaces';

@Injectable({
    providedIn: 'root',
})
export class StorageContract implements Storage {
    private _initialised: boolean;

    constructor(private storage: IonicStorage) {}

    async find(key: string): Promise<any> {
        await this.init();
        return await this.storage.get(key);
    }

    async set(key: string, value: any): Promise<any> {
        await this.init();
        return this.storage.set(key, value);
    }

    async remove(key: string): Promise<any> {
        await this.init();
        return this.storage.remove(key);
    }

    async keys(): Promise<string[]> {
        await this.init();
        return this.storage.keys();
    }

    private async init(): Promise<void> {
        if (this._initialised) return;

        await this.storage.create();
        this._initialised = true;
    }
}
