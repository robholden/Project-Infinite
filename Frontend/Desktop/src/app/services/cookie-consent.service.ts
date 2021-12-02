import { Injectable } from '@angular/core';

import { CookieService } from 'ngx-cookie-service';

@Injectable({
    providedIn: 'root',
})
export class CookieConsentService {
    name: string = 'COOKIES_ALLOWED';
    enabled = true;
    doNotTrack = false;

    constructor(private cookieService: CookieService) {
        // Run can store cookie method
        this.enabled = this.canStoreCookies();

        // Set do not track property
        if (navigator && ((navigator as any).doNotTrack || '') !== '') {
            const noTrack = (navigator as any).doNotTrack;
            this.doNotTrack = noTrack === 'yes' || noTrack === '1';
        }
    }

    enableCookies(): void {
        const expires = new Date();
        expires.setDate(expires.getDate() + 365);
        this.cookieService.set(this.name, 'true', expires, null, null, null, 'Lax');
        this.enabled = true;
    }

    disableCookies(): void {
        this.cookieService.delete(this.name);
        this.enabled = false;
        this.canStoreCookies();
    }

    canStoreCookies(): boolean {
        const law = this.cookieService.get(this.name);
        if (!law || law !== 'true') {
            const cookies = this.cookieService.getAll();
            const allowed = ['token', 'refresh_token', 'auth_token', 'expires'];
            for (const cookie in cookies) {
                if (allowed.indexOf(cookie) > -1) continue;
                this.cookieService.delete(cookie);
            }
            return false;
        }

        return true;
    }
}
