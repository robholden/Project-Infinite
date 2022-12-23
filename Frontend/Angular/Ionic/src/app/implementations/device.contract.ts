import { Injectable } from '@angular/core';

import { Device } from '@shared/interfaces';

import { Device as CapDevice } from '@capacitor/device';

@Injectable({
    providedIn: 'root',
})
export class DeviceContract implements Device {
    private _uuid: string;

    constructor() {}

    async uuid(): Promise<string> {
        if (!this._uuid) this._uuid = (await CapDevice.getId()).uuid;
        return this._uuid;
    }
}
