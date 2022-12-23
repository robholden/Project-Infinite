import { Component, Injector } from '@angular/core';

import { Platform } from '@ionic/angular';

import { SiteLoaderService } from '@shared/services';
import { AppState } from '@shared/storage';

import { setupEvents } from './functions/setup-events.fn';

import { SplashScreen } from '@capacitor/splash-screen';
import { StatusBar, Style } from '@capacitor/status-bar';

@Component({
    selector: 'sc-root',
    templateUrl: 'app.component.html',
    styleUrls: ['app.component.scss'],
})
export class AppComponent {
    loaded = false;
    darkMode: boolean = null;

    constructor(public siteLoader: SiteLoaderService, private platform: Platform, private injector: Injector, public appState: AppState) {
        this.initializeApp();
    }

    async initializeApp() {
        // Wait for app to be ready
        await this.platform.ready();

        if (this.platform.is('cordova')) StatusBar.setStyle({ style: Style.Dark });

        // Setup our in app events
        setupEvents(this.injector);

        // Run site loader
        await this.siteLoader.loadStorage();
        this.siteLoader.connectToServer();

        // Finally, show the screen
        this.loaded = true;
        SplashScreen.hide();
    }
}
