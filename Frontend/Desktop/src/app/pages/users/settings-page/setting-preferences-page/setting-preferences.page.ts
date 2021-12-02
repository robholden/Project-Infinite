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
    prefs: { key: string; value: boolean }[] = [];

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
            this.prefs = Object.keys(settings).map((key) => ({ key, value: settings[key] }));
        }
    }

    async toggle(key: string, state: boolean) {
        // Toggle ui
        this.prefs[key] = state;

        // Create prefs model
        const prefs = this.settings;
        prefs[key] = state;

        // Go to api
        const resp = await this.commsService.updateSettings(prefs);

        // Revert back when there's an error
        this.prefs[key] = resp instanceof CustomError ? !state : state;
    }
}
