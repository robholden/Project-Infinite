import { Injectable } from '@angular/core';

import { isNull } from '@shared/functions';
import { Storage, StorageOptions } from '@shared/interfaces';

import { getCookie, getCookies, removeCookie, setCookie } from 'typescript-cookie';

@Injectable({
    providedIn: 'root',
})
export class StorageContract implements Storage {
    find<TValue>(key: string, options: StorageOptions<TValue>): Promise<TValue> {
        let value: string;
        if (options.store?.includes('cookie')) value = getCookie(key);
        else if (options.store?.includes('session')) value = sessionStorage.getItem(key);
        else if (options.store?.includes('local')) localStorage.get(key);
        else return Promise.resolve(options.defaultValue);

        try {
            return Promise.resolve(this.parse<TValue>(value));
        } catch {
            return Promise.resolve(options.defaultValue);
        }
    }

    set<TValue>(key: string, value: TValue, options: StorageOptions<TValue>): Promise<void> {
        if (isNull(value)) return this.remove(key, options);

        const stringData = this.toJson(value);

        if (options.store?.includes('cookie')) setCookie(key, stringData, { expires: options.expiresIn, path: '/', sameSite: 'Strict' });
        if (options.store?.includes('session')) sessionStorage.setItem(key, stringData);
        if (options.store?.includes('local')) localStorage.setItem(key, stringData);

        return Promise.resolve();
    }

    remove(key: string, options: StorageOptions): Promise<void> {
        if (options.store.includes('cookie')) removeCookie(key, { path: '/' });
        if (options.store.includes('session')) sessionStorage.removeItem(key);
        else if (options.store.includes('local')) localStorage.removeItem(key);

        return Promise.resolve();
    }

    keys(): Promise<string[]> {
        return Promise.resolve([...Object.keys(localStorage), ...Object.keys(sessionStorage), ...Object.keys(getCookies())]);
    }

    private toJson(value: any): string {
        if (value === null || value === undefined) return null;
        return typeof value === 'string' ? value : JSON.stringify(value);
    }

    private parse<TValue>(value: string): TValue {
        if (value === null || value === undefined) return null;

        try {
            return JSON.parse(value);
        } catch (err) {
            return value as any;
        }
    }
}
