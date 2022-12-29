import { inject } from '@angular/core';

import { BehaviorSubject, Observable, Subject, Subscription } from 'rxjs';

import { pascalToScores } from '@shared/functions';
import { INJ_STORAGE } from '@shared/injectors';
import { Storage, StorageOptions } from '@shared/interfaces';
import { SMap } from '@shared/models';

export type IsArray<T> = T extends Array<unknown> ? T : never;

export type StorageConfig<TSchema> = Record<keyof TSchema, StorageOptions<TSchema[keyof TSchema]>>;

class CustomBaseStore<TSchema> {
    private state: SMap<any> = {};
    private signals: SMap<any> = {};
    private subjects: SMap<Subject<unknown>> = {};
    private behaviours: SMap<BehaviorSubject<unknown>> = {};

    constructor() {}

    snapshot<U extends keyof TSchema>(key: U, def?: TSchema[U]): TSchema[U] {
        const stateKey = key as string;
        if (stateKey in this.signals) {
            return this.signals[stateKey].value;
        }

        return def;
    }

    snapshotBy<U extends keyof TSchema, O extends IsArray<TSchema[U]>>(key: U, comparer: (value: O[number]) => boolean): O[number] {
        const state = this.state[key as string];
        if (!state) return undefined;

        return state.find(comparer);
    }

    select<U extends keyof TSchema>(key: U): { value: TSchema[U] } {
        if (key in this.signals) {
            return this.signals[key as string];
        }

        return this.createSignal(key as string);
    }

    set<U extends keyof TSchema>(key: U, updatedValue: TSchema[U]): CustomBaseStore<TSchema> {
        const slice = this.select(key);
        slice.value = updatedValue;
        return this;
    }

    update<U extends keyof TSchema>(key: U, newValue: (item: TSchema[U]) => TSchema[U]): CustomBaseStore<TSchema> {
        const slice = this.select(key);
        slice.value = newValue(slice.value);
        return this;
    }

    observe<U extends keyof TSchema>(key: U, fireImmediately: boolean = true): Observable<TSchema[U]> {
        const subjects = fireImmediately ? this.behaviours : this.subjects;

        let subject = subjects[key as string] as Subject<TSchema[U]>;
        if (subject) return subject.asObservable() as Observable<TSchema[U]>;

        subject = fireImmediately ? new BehaviorSubject<TSchema[U]>(this.state[key as string]) : new Subject<TSchema[U]>();
        subjects[key as string] = subject as Subject<unknown>;

        return subject.asObservable();
    }

    removeMany<U extends keyof TSchema>(...keys: U[]): CustomBaseStore<TSchema> {
        (keys || []).forEach((key) => {
            if (!(key in this.state)) return;

            this.state[key as string] = null;
            if (key in this.behaviours) this.behaviours[key as string].next(null);
            if (key in this.subjects) this.subjects[key as string].next(null);
        });

        return this;
    }

    remove<U extends keyof TSchema>(key: U): CustomBaseStore<TSchema> {
        if (!(key in this.state)) return this;

        this.state[key as string] = null;
        if (key in this.behaviours) this.behaviours[key as string].next(null);
        if (key in this.subjects) this.subjects[key as string].next(null);

        return this;
    }

    private createSignal<U extends keyof TSchema>(key: string): { value: TSchema[U] } {
        const signal: any = Object.defineProperty({}, 'value', {
            get: () => this.state[key],
            set: (updatedValue: TSchema[U]) => {
                this.state[key] = updatedValue;
                if (key in this.behaviours) this.behaviours[key].next(updatedValue);
                if (key in this.subjects) this.subjects[key].next(updatedValue);
            },
        });

        this.signals[key] = signal;
        return signal;
    }
}

export class CustomStore<TSchema> extends CustomBaseStore<TSchema> {
    private storage = inject<Storage>(INJ_STORAGE);
    private onload = new BehaviorSubject<boolean>(false);

    constructor(private config?: Partial<StorageConfig<TSchema>>) {
        super();

        if (this.config) this.persist();
    }

    async init(): Promise<void> {
        if (this.onload.value) {
            return;
        }

        let observer: Subscription | undefined;
        const prom = new Promise<void>((res) => {
            observer = this.onload.subscribe((success) => {
                if (success) res();
            });
        });

        await prom;

        observer?.unsubscribe();
    }

    async retrieve<U extends keyof TSchema>(key: U, def?: TSchema[U]): Promise<TSchema[U]> {
        const options = this.config[key];
        if (!options) return this.snapshot(key, def);

        // Add store name as prefix
        const stateKey = key as string;
        const storeKey = this.getKeyName(stateKey);

        // Get value from storage
        return await this.storage.find(storeKey, { ...options, defaultValue: def });
    }

    set<U extends keyof TSchema>(key: U, updatedValue: TSchema[U], options?: StorageOptions<TSchema[U]>): CustomStore<TSchema> {
        if (options) {
            this.config[key as string] = {
                ...(this.config[key as string] || {}),
                ...options,
            };
        }

        super.set(key, updatedValue);

        return this;
    }

    private async persist() {
        const proms = Object.entries<StorageOptions<any>>(this.config).map(async ([key, options]) => {
            // Fetch value if promise
            let value = options.defaultValue;
            if (typeof value === 'function') {
                value = await value();
            }

            // Add store name as prefix
            const storeKey = this.getKeyName(key);

            // Find data from storage
            const data = options.store ? await this.storage.find(storeKey, options) : value;

            // Add value as default
            const slice = this.select(key as keyof TSchema);
            slice.value = data;

            // Watch for changes and update storage
            if (options.store) {
                const watcher = this.observe(key as keyof TSchema, false);
                watcher.subscribe(async (value) => await this.storage.set(storeKey, value, this.config[key as string]));
            }
        });

        await Promise.all(proms);

        this.onload.next(true);
    }

    private getKeyName(key: string) {
        const options = this.config[key];
        return pascalToScores(options?.keyName ?? key);
    }
}
