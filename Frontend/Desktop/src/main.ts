import { enableProdMode, LOCALE_ID } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { TranslateService } from '@ngx-translate/core';

import { locale } from '@shared/helpers';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

import cssVars from 'css-vars-ponyfill';
import tippy from 'tippy.js';

cssVars();
tippy('[data-tooltip]');

if (environment.production) {
    enableProdMode();
}

platformBrowserDynamic()
    .bootstrapModule(AppModule, {
        providers: [{ provide: LOCALE_ID, useFactory: (translate: TranslateService) => translate.currentLang || locale, deps: [TranslateService] }],
    })
    .catch((err) => console.error(err));
