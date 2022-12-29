import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { TranslateModule } from '@ngx-translate/core';

import { PipesModule } from '@shared/pipes/pipes.module';

import { DirectivesModule } from '@app/directives/directives.module';
import { FeaturesModule } from '@app/features/features.module';

import { LoginComponent } from './auth/login/login.component';
import { RegisterExternalComponent } from './auth/register-external/register-external.component';
import { RegisterComponent } from './auth/register/register.component';
import { TwoFactorComponent } from './auth/two-factor/two-factor.component';
import { FooterComponent } from './footer/footer.component';
import { HeaderComponent } from './header/header.component';
import { NotificationsComponent } from './notifications/notifications.component';

@NgModule({
    imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, FeaturesModule, PipesModule, DirectivesModule],
    declarations: [
        HeaderComponent,
        FooterComponent,
        NotificationsComponent,
        LoginComponent,
        RegisterComponent,
        RegisterExternalComponent,
        TwoFactorComponent,
    ],
    exports: [
        TranslateModule,
        PipesModule,
        DirectivesModule,
        HeaderComponent,
        FooterComponent,
        NotificationsComponent,
        LoginComponent,
        RegisterComponent,
        RegisterExternalComponent,
        TwoFactorComponent,
    ],
})
export class ComponentsModule {}
