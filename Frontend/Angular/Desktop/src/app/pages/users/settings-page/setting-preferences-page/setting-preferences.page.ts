import { Component, OnInit } from '@angular/core';

type TodoPrefs = {};

@Component({
    selector: 'sc-setting-preferences',
    templateUrl: './setting-preferences.page.html',
    styleUrls: ['./setting-preferences.page.scss'],
})
export class SettingPreferencesPage implements OnInit {
    errored: boolean = null;
    settings: TodoPrefs;

    emailPrefs: string[] = [];

    constructor() {}

    ngOnInit() {
        this.load();
    }

    private async load() {}

    async toggle(key: string, state: boolean) {
        this.settings[key] = state;
        // const resp = await this.commsService.updateSettings(this.settings);
        // this.settings[key] = resp instanceof CustomError ? !state : state;
    }
}
