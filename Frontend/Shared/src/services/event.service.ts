import { Injectable } from '@angular/core';

import { CustomEvent } from '@shared/enums';

@Injectable({
    providedIn: 'root',
})
export class EventService {
    private events: { key?: string; name: string; fn: (params?: { [key: string]: any }) => any | Promise<any> }[] = [];

    constructor() {}

    /**
     * Registers a function with a specific event
     *
     * @param event The type of event
     * @param fn The function to call when raised
     */
    register<T extends any>(event: CustomEvent, fn: (params?: { [key: string]: any }) => void | Promise<T>, key?: string): void {
        const name = CustomEvent[event];
        const entry = {
            key,
            name,
            fn,
        };

        let i = -1;
        if (key) i = this.events.findIndex((e) => e.key === key && e.name === name);

        if (i >= 0) this.events[i] = entry;
        else this.events.push(entry);
    }

    /**
     * Triggers a specified event
     *
     * @param event The type of event
     * @param params Any params the calling function needs
     * @param callback A function ran upon completion
     */
    async trigger<T extends any>(event: CustomEvent, params: { [key: string]: any } = null): Promise<T> {
        return new Promise<T>(async (res) => {
            const name = CustomEvent[event];
            const instance = this.events.find((s) => s.name === name);
            if (!instance) {
                res(null);
                return;
            }

            const result = await instance.fn(params);
            res(result);
        });
    }
}
