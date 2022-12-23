import { Injectable } from '@angular/core';

import { Platform } from '@shared/interfaces/platform.interface';

import { detect } from 'detect-browser';

@Injectable({
    providedIn: 'root',
})
export class PlatformContract implements Platform {
    constructor() {}

    get name(): string {
        try {
            const browser = detect();
            return [browser.name, browser.os, browser.version].join(' ');
        } catch {}

        return 'Browser';
    }
}
