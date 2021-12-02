import { Inject, Injectable } from '@angular/core';

import { BehaviorSubject, Observable } from 'rxjs';

import { nullCheck } from '@shared/functions';
import { INJ_STORAGE } from '@shared/injectors';
import { Storage, StorageOptions } from '@shared/interfaces';
import { SMap } from '@shared/models';

class StorageState {
    constructor(public value: any, public opts?: StorageOptions) {}
}

@Injectable({
    providedIn: 'root',
})
export class StorageService<Store> {
    private _state: SMap<StorageState> = {};
    private _subjects: SMap<BehaviorSubject<any>> = {};

    constructor(@Inject(INJ_STORAGE) private storage: Storage) {}

    /**
     * Clears non-persistant storage items
     */
    async clear() {
        const keys = await this.storage.keys();
        const persistedKeys = Object.keys(this._state).filter((k) => this._state[k].opts.persisted);

        keys.filter((k) => !persistedKeys.includes(k)).forEach(async (key) => await this.storage.remove(key));
    }

    /**
     * Initialises the storage for a given key
     *
     * @param key The storage key
     * @param opts What storage options to use
     * @param defaultValue The fallback value if the key is missing
     */
    async load<T>(key: keyof Store, opts?: StorageOptions, defaultValue?: T): Promise<void> {
        const value = await this.storage.get<T>(key as string, opts);
        this._state[key as string] = new StorageState(nullCheck(value, defaultValue), opts);
    }

    /**
     * Returns a value from storage for a given key
     *
     * @param key The storage key
     */
    get<T>(key: keyof Store): T {
        return this._state[key as string]?.value;
    }

    /**
     * Returns a value directly from storage for a given key
     *
     * @param key The storage key
     */
    async getFromStorage<T>(key: keyof Store): Promise<T> {
        return await this.storage.get<T>(key as string, this._state[key as string]?.opts);
    }

    /**
     * Returns an observable for a storage item that triggers when the value has changed
     *
     * @param key The storage key
     */
    watch<T>(key: keyof Store): Observable<T> {
        let subject = this._subjects[key as string];
        if (!subject) {
            subject = new BehaviorSubject<T>(this.get<T>(key));
            this._subjects[key as string] = subject;
        }

        return subject.asObservable();
    }

    /**
     * Updates the value by a given key
     *
     * @param key The storage key
     * @param value The new value
     * @param opts What storage options to use
     */
    async set(key: keyof Store, value: any, opts?: StorageOptions): Promise<void> {
        const state = this._state[key as string] || new StorageState(value);
        if (opts) state.opts = opts;
        state.value = value;

        this._state[key as string] = state;
        await this.storage.set(key as string, value, state.opts);

        this._subjects[key as string]?.next(value);
    }

    /**
     * Removes an item or items from storage by a keys
     *
     * @param keys The storage key
     */
    remove(...keys: (keyof Store)[]): void {
        keys.forEach((key: keyof Store) => {
            this.storage.remove(key as string, this._state[key as string]?.opts);

            this._state[key as string].value = null;
            this._subjects[key as string]?.next(null);
        });
    }

    /**
     * Removes an item only from storage by a given key
     *
     * @param key The storage key
     */
    async removeFromStorage(key: keyof Store): Promise<void> {
        await this.storage.remove(key as string);
    }
}
