import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, HostListener, Injector, isDevMode, OnInit } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { Router, RoutesRecognized } from '@angular/router';

import { filter } from 'rxjs/operators';

import { TranslateService } from '@ngx-translate/core';

import { environment } from '@env/environment';

import { locale } from '@shared/helpers';
import { SiteLoaderService } from '@shared/services';
import { AuthState } from '@shared/storage';

import { fade } from './functions/animations.fn';
import { routingTitles } from './functions/routing-titles.fn';
import { setupEvents } from './functions/setup-events.fn';
import { CookieConsentService } from './services/cookie-consent.service';
import { DesktopStore } from './storage/desktop.store';

@Component({
    selector: 'sc-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    animations: [fade, trigger('router', [state('in', style({ opacity: 1 })), transition(':enter', [style({ opacity: 0 }), animate(250)])])],
})
export class AppComponent implements OnInit {
    loaded: boolean = false;
    nav_loaded: boolean;
    env = environment;

    get healthy() {
        return this.loaded && this.siteLoader.healthy;
    }

    constructor(
        injector: Injector,
        translate: TranslateService,
        public authState: AuthState,
        public siteLoader: SiteLoaderService,
        private title: Title,
        private router: Router,
        private meta: Meta,
        private cookieConsent: CookieConsentService,
        private desktopStore: DesktopStore
    ) {
        translate.use(locale);

        routingTitles(this.title, this.router, this.meta);
        setupEvents(injector);

        window.onbeforeunload = () => this.cookieConsent.canStoreCookies();

        router.events.pipe(filter((event) => event instanceof RoutesRecognized)).subscribe(() => (this.nav_loaded = true));
    }

    ngOnInit() {
        this.load();
    }

    async load() {
        await this.siteLoader.loadStorage();
        this.siteLoader.connectToServer();
        this.loaded = true;
    }

    reload() {
        if (!isDevMode) return;

        this.siteLoader.connectToServer();
    }

    @HostListener('window:beforeunload', ['$event'])
    onBeforeUnload(event: any) {
        if (this.desktopStore.shouldPreventRefresh) {
            event.preventDefault();
            event.returnValue = 'Changes that you made may not be saved.';
        }
    }
}
