import { Injectable } from '@angular/core';

import { getCookie, getCookies, removeCookie, setCookie } from 'typescript-cookie';

@Injectable({
    providedIn: 'root',
})
export class CookieConsentService {
    name: string = 'COOKIES_ALLOWED';
    enabled = true;
    doNotTrack = false;

    constructor() {
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
        setCookie(this.name, 'true', { expires, sameSite: 'Lax' });
        this.enabled = true;
    }

    disableCookies(): void {
        removeCookie(this.name);
        this.enabled = false;
        this.canStoreCookies();
    }

    canStoreCookies(): boolean {
        const law = getCookie(this.name);
        if (!law || law !== 'true') {
            const cookies = getCookies();
            const allowed = ['token', 'refresh_token', 'auth_token', 'expires'];
            for (const cookie in cookies) {
                if (allowed.indexOf(cookie) > -1) continue;
                removeCookie(cookie);
            }
            return false;
        }

        return true;
    }
}
