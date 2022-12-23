import { Component, OnInit } from '@angular/core';

import { CookieConsentService } from '@app/services/cookie-consent.service';

@Component({
    selector: 'sc-policy-cookies',
    templateUrl: 'policy-cookies.page.html',
    styleUrls: ['policy-cookies.page.scss'],
})
export class PolicyCookiesPage implements OnInit {
    constructor(public cookieConsent: CookieConsentService) {}

    ngOnInit() {}
}
