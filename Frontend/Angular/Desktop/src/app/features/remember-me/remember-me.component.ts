import { Component, HostBinding, OnInit } from '@angular/core';

import { isNull } from '@shared/functions';
import { AuthState } from '@shared/storage';

import { CookieConsentService } from '@app/services/cookie-consent.service';

@Component({
    selector: 'sc-remember-me',
    templateUrl: './remember-me.component.html',
    styleUrls: ['remember-me.component.scss'],
})
export class RememberMeComponent implements OnInit {
    private _hidden: boolean;

    @HostBinding('class.hidden') get hidden(): boolean {
        return this._hidden || !this.cookieConsent.enabled;
    }

    constructor(private authState: AuthState, private cookieConsent: CookieConsentService) {}

    ngOnInit() {
        this.updateDisplay();
    }

    set(state: boolean) {
        this.authState.setRememberMe(state);
        this.updateDisplay();
    }

    private updateDisplay() {
        this._hidden = !isNull(this.authState.snapshot('rememberMe'));
    }
}
