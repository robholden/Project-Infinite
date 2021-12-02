import { Injectable, Injector } from '@angular/core';

import { isNull } from '@shared/functions';
import { Storage, StorageOptions } from '@shared/interfaces';
import { SMap } from '@shared/models';

import { CookieService } from 'ngx-cookie-service';

@Injectable({
    providedIn: 'root',
})
export class StorageContract implements Storage {
    private cookieService: CookieService;
    private expirations: SMap<Date> = {};

    constructor(injector: Injector) {
        this.cookieService = injector.get<CookieService>(CookieService);
    }

    get<T>(key: string, opts?: StorageOptions): Promise<T> {
        let value: string;
        if (opts?.cookie) value = this.cookieService.get(key);
        else {
            if (opts?.expiresIn instanceof Number) {
                const d = this.expirations[key];
                if (d < new Date()) {
                    console.log(key + ' expired after ' + opts?.expiresIn + 's');

                    return null;
                }
            }

            value = opts?.persisted ? localStorage.getItem(key) : sessionStorage.getItem(key);
        }
        return Promise.resolve(this.parse(value));
    }

    set<T>(key: string, value: T, opts?: StorageOptions): Promise<void> {
        if (isNull(value)) {
            this.remove(key, opts);
            return Promise.resolve();
        }

        let storedValue: any = value;
        if (value instanceof Object) storedValue = JSON.stringify(value);

        if (opts?.cookie) this.cookieService.set(key, storedValue, opts.expiresIn, '/', null, null, 'Strict');
        else {
            if (opts?.persisted) localStorage.setItem(key, storedValue);
            else sessionStorage.setItem(key, storedValue);

            if (opts?.expiresIn instanceof Number) {
                const d = new Date();
                d.setSeconds(new Date().getSeconds() + (opts.expiresIn as number));
                this.expirations[key] = d;
            }
        }

        return Promise.resolve();
    }

    remove(key: string, opts?: StorageOptions): Promise<void> {
        if (opts?.cookie) this.cookieService.delete(key, '/');
        else if (opts?.persisted) localStorage.removeItem(key);
        else sessionStorage.removeItem(key);

        return Promise.resolve();
    }

    keys(): Promise<string[]> {
        return Promise.resolve([...Object.keys(localStorage), ...Object.keys(sessionStorage), ...Object.keys(this.cookieService.getAll())]);
    }

    private parse<T>(value: any): T {
        try {
            value = JSON.parse(value);
        } catch (err) {}

        return value;
    }
}
