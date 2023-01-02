import { Injectable } from '@angular/core';

import { CustomStore } from '@shared/storage';

type DesktopSchema = {};

type PreventReload = {
    id?: string;
    fn: () => boolean;
};

@Injectable({
    providedIn: 'root',
})
export class DesktopStore extends CustomStore<DesktopSchema> {
    private preventReloaders: PreventReload[] = [];

    constructor() {
        super();
    }

    setCanReload({ id, fn }: PreventReload) {
        const i = this.preventReloaders.findIndex((pr) => pr.id === id);

        if (i < 0) this.preventReloaders.push({ id, fn });
        else this.preventReloaders[i] = { id, fn };
    }

    unsetCanReload(id?: string) {
        if (!id) this.preventReloaders.pop();
        else {
            const i = this.preventReloaders.findIndex((pr) => pr.id === id);
            if (i >= 0) this.preventReloaders.slice(i, 1);
        }
    }

    canRefresh() {
        if (this.preventReloaders.length === 0) return true;

        return this.preventReloaders.every((pr) => pr.fn());
    }
}
