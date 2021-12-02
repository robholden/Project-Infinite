import { Component } from '@angular/core';

import { TranslateService } from '@ngx-translate/core';

import { environment } from '@env/environment';

import { setLocale } from '@shared/helpers';
import { SiteLoaderService } from '@shared/services';
import { AppState } from '@shared/storage';

@Component({
    selector: 'sc-footer',
    templateUrl: './footer.component.html',
    styleUrls: ['footer.component.scss'],
})
export class FooterComponent {
    env = environment;

    constructor(public siteLoader: SiteLoaderService, public appState: AppState, public translate: TranslateService) {}

    updateLang(lang: string) {
        setLocale(lang);
        this.translate.use(lang);
        if (this.translate.langs.includes(lang)) setTimeout(() => this.appState.updateTrx(), 0);
    }
}
