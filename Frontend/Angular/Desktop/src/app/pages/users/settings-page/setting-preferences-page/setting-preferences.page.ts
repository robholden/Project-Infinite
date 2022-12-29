import { Component, OnDestroy, OnInit } from '@angular/core';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { UserPrefs } from '@shared/models';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'pi-setting-preferences',
    templateUrl: './setting-preferences.page.html',
    styleUrls: ['./setting-preferences.page.scss'],
})
export class SettingPreferencesPage implements OnInit, OnDestroy {
    private destroy$ = new Subject();

    settings: UserPrefs;
    emailPrefs: (keyof UserPrefs)[] = ['marketingEmails'];

    constructor(private authState: AuthState, private service: UserService) {}

    ngOnInit() {
        this.authState
            .observe('user')
            .pipe(takeUntil(this.destroy$))
            .subscribe((user) => (this.settings = user.preferences || { marketingEmails: false }));
    }

    ngOnDestroy(): void {
        this.destroy$.next();
    }

    async toggle(key: string, state: boolean) {
        this.settings[key] = state;
        const resp = await this.service.updatePreferences(this.settings);
        if (!resp) this.settings[key] = !state;
    }
}
