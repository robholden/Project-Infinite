import { Component, OnDestroy } from '@angular/core';

import { Subscription } from 'rxjs';

import { TranslateService } from '@ngx-translate/core';

import { environment } from '@env/environment';

import { setLocale } from '@shared/helpers';
import { TrxPipe } from '@shared/pipes/trx.pipe';
import { SiteLoaderService } from '@shared/services';
import { AppState } from '@shared/storage';

interface TranslatedLanguage {
    locale: string;
    name: string;
}

@Component({
    selector: 'sc-footer',
    templateUrl: './footer.component.html',
    styleUrls: ['footer.component.scss'],
    providers: [TrxPipe],
})
export class FooterComponent implements OnDestroy {
    private sub: Subscription;

    langs: TranslatedLanguage[] = [];

    readonly env = environment;

    constructor(public siteLoader: SiteLoaderService, public appState: AppState, public translate: TranslateService, private trx: TrxPipe) {
        this.sub = this.appState.language$.subscribe(() => this.setLangs());
        this.setLangs();
    }

    ngOnDestroy(): void {
        this.sub?.unsubscribe();
    }

    updateLang(lang: string) {
        setLocale(lang);
        this.translate.use(lang);
        if (this.translate.langs.includes(lang)) setTimeout(() => this.appState.updateTrx(), 0);
    }

    private setLangs() {
        this.langs = ['en-gb', 'en-us', 'de', 'fr', 'es'].map((locale) => ({ locale, name: this.trx.transform(`locales.${locale}`) }));
    }
}
