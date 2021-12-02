import { Component, HostBinding, OnInit } from '@angular/core';

import { CookieConsentService } from '@app/services/cookie-consent.service';

@Component({
    selector: 'sc-cookie-consent',
    templateUrl: './cookie-consent.component.html',
    styleUrls: ['cookie-consent.component.scss'],
})
export class CookieConsentComponent implements OnInit {
    @HostBinding('class.hidden') hidden: boolean = true;

    constructor(private cookieConsent: CookieConsentService) {
        this.updateDisplay();
    }

    ngOnInit() {}

    accept() {
        this.cookieConsent.enableCookies();
        this.updateDisplay();
    }

    private updateDisplay() {
        this.hidden = this.cookieConsent.enabled;
    }
}
