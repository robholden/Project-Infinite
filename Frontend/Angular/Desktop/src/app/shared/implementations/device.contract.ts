import { Injectable } from '@angular/core';

import { sha256 } from 'js-sha256';

import { isPrivateMode } from '@shared/functions';
import { Device } from '@shared/interfaces';

import { PlatformContract } from './platform.contract';

@Injectable({
    providedIn: 'root',
})
export class DeviceContract implements Device {
    private _uuid: string;

    constructor(private platform: PlatformContract) {}

    async uuid(): Promise<string> {
        if (!this._uuid) {
            const is_private = await isPrivateMode();
            const values = [this.platform.name, is_private].join(' ');
            this._uuid = sha256(values);
        }

        return this._uuid;
    }
}
