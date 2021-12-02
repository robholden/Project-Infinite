import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouteReuseStrategy } from '@angular/router';

import { TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';

import { Facebook } from '@ionic-native/facebook/ngx';
import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { GooglePlus } from '@ionic-native/google-plus/ngx';
import { SignInWithApple } from '@ionic-native/sign-in-with-apple/ngx';
import { IonicModule, IonicRouteStrategy } from '@ionic/angular';
import { IonicStorageModule } from '@ionic/storage-angular';

import { environment } from '@env/environment';

import { AppTranslateLoader, locale } from '@shared/helpers';
import { INJ_DEVICE, INJ_ENV, INJ_PLATFORM, INJ_STORAGE, INJ_TOAST, INJ_TRANSLATION } from '@shared/injectors';
import { AppInterceptor } from '@shared/middleware';
import { AppState } from '@shared/storage';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DeviceContract } from './implementations/device.contract';
import { PlatformContract } from './implementations/platform.contract';
import { StorageContract } from './implementations/storage.contract';
import { ToastContract } from './implementations/toast.contract';
import { PrivacyPolicyModal } from './modals/privacy-policy-modal/privacy-policy.modal';
import { TermsAndConditionsModal } from './modals/terms-and-conditions-modal/terms-and-conditions.modal';

import { MarkdownModule } from 'ngx-markdown';

@NgModule({
    declarations: [AppComponent],
    entryComponents: [TermsAndConditionsModal, PrivacyPolicyModal],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        ReactiveFormsModule,
        HttpClientModule,
        IonicModule.forRoot(),
        IonicStorageModule.forRoot(),
        MarkdownModule.forRoot(),
        AppRoutingModule,
        TranslateModule.forRoot({
            defaultLanguage: locale,
            useDefaultLang: true,
            loader: {
                provide: TranslateLoader,
                useClass: AppTranslateLoader,
                deps: [HttpClient, AppState],
            },
        }),
    ],
    providers: [
        { provide: INJ_TRANSLATION, useClass: TranslateService },
        { provide: INJ_ENV, useValue: environment },
        { provide: INJ_STORAGE, useClass: StorageContract },
        { provide: INJ_DEVICE, useClass: DeviceContract },
        { provide: INJ_PLATFORM, useClass: PlatformContract },
        { provide: INJ_TOAST, useClass: ToastContract },

        {
            provide: HTTP_INTERCEPTORS,
            useClass: AppInterceptor,
            multi: true,
        },
        { provide: RouteReuseStrategy, useClass: IonicRouteStrategy },
        FingerprintAIO,
        GooglePlus,
        Facebook,
        SignInWithApple,
    ],
    bootstrap: [AppComponent],
})
export class AppModule {}
