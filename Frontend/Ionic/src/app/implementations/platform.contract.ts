import { Injectable } from '@angular/core';

import { Platform as IonicPlatform } from '@ionic/angular';

import { Platform } from '@shared/interfaces';

@Injectable({
    providedIn: 'root',
})
export class PlatformContract implements Platform {
    constructor(private platform: IonicPlatform) {}

    get name(): string {
        return ['Ionic', ...this.platform.platforms()].join(' ');
    }
}
