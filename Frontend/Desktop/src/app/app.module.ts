import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';

import { environment } from '@env/environment';

import { AppTranslateLoader, locale } from '@shared/helpers';
import { INJ_ENV, INJ_TRANSLATION } from '@shared/injectors';
import { AppInterceptor } from '@shared/middleware';
import { AppState } from '@shared/storage';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ComponentsModule } from './components/component.module';
import { SharedModule } from './shared/shared.module';

import { MarkdownModule } from 'ngx-markdown';

@NgModule({
    declarations: [AppComponent],
    imports: [
        BrowserModule.withServerTransition({ appId: 'serverApp' }),
        BrowserAnimationsModule,
        HttpClientModule,
        AppRoutingModule,
        SharedModule,
        ComponentsModule,
        MarkdownModule.forRoot(),
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
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AppInterceptor,
            multi: true,
        },
    ],
    exports: [TranslateModule],
    bootstrap: [AppComponent],
})
export class AppModule {}
