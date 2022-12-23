import { HttpClient } from '@angular/common/http';

import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { TranslateLoader } from '@ngx-translate/core';

import { AppState } from '@shared/storage';

import * as _merge from 'lodash.merge';

function getLocale() {
    const lang = localStorage.getItem('lang');
    if (lang) return lang;

    if (navigator.languages != undefined) return navigator.languages[0];
    else return navigator.language;
}

export function setLocale(lang: string) {
    localStorage.setItem('lang', lang);
}

export const locale = getLocale();

export class AppTranslateLoader implements TranslateLoader {
    constructor(private http: HttpClient, private appState: AppState) {}

    getTranslation(lang: string): Observable<any> {
        lang = (lang || '').toLowerCase();

        const default_trx = this.http.get<any>(`assets/i18n/_defaults.json?t=${new Date().getTime()}`);
        const current_trx = this.http.get<any>(`assets/i18n/${lang}.json?t=${new Date().getTime()}`);

        return combineLatest([default_trx, current_trx]).pipe(
            map((results) => {
                const def = results[0];
                const cur = results[1];

                setTimeout(() => this.appState.updateTrx(), 0);
                return _merge(def, cur);
            })
        );
    }
}
