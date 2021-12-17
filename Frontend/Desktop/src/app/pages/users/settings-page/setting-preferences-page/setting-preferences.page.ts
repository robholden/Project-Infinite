import { Component, OnInit } from '@angular/core';

import { CommSettings, CustomError } from '@shared/models';
import { CommsService } from '@shared/services/comms';

@Component({
    selector: 'sc-setting-preferences',
    templateUrl: './setting-preferences.page.html',
    styleUrls: ['./setting-preferences.page.scss'],
})
export class SettingPreferencesPage implements OnInit {
    errored: boolean = null;
    settings: CommSettings;

    emailPrefs: string[] = [];

    constructor(private commsService: CommsService) {}

    ngOnInit() {
        this.load();
    }

    private async load() {
        const settings = await this.commsService.getSettings();
        if (settings instanceof CustomError || !settings) this.errored = true;
        else {
            this.settings = settings;
            this.errored = false;

            const prefKeys = Object.keys(settings);
            this.emailPrefs = prefKeys.filter((key) => key.toLowerCase().includes('email'));
        }
    }

    async toggle(key: string, state: boolean) {
        this.settings[key] = state;
        const resp = await this.commsService.updateSettings(this.settings);
        this.settings[key] = resp instanceof CustomError ? !state : state;
    }
}
