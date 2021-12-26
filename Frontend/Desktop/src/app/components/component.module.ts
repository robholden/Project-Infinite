import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { TranslateModule } from '@ngx-translate/core';

import { PipesModule } from '@shared/pipes/pipes.module';

import { DirectivesModule } from '@app/directives/directives.module';
import { FeaturesModule } from '@app/features/features.module';

import { ApprovalComponent } from './approval-component/approval.component';
import { LoginComponent } from './auth/login/login.component';
import { RegisterExternalComponent } from './auth/register-external/register-external.component';
import { RegisterComponent } from './auth/register/register.component';
import { TwoFactorComponent } from './auth/two-factor/two-factor.component';
import { FooterComponent } from './footer/footer.component';
import { HeaderComponent } from './header/header.component';
import { NotificationsComponent } from './notifications/notifications.component';
import { PictureMatchesComponent } from './picture-matches/picture-matches.component';
import { PicturePopupComponent } from './picture-popup/picture-popup.component';
import { SelectCollectionComponent } from './show-picture/select-collection/select-collection.component';
import { ShowPictureComponent } from './show-picture/show-picture.component';
import { ShowPicturesComponent } from './show-pictures/show-pictures.component';

import { LazyLoadImageModule } from 'ng-lazyload-image';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';

@NgModule({
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        FeaturesModule,
        LazyLoadImageModule,
        PipesModule,
        DirectivesModule,
        InfiniteScrollModule,
    ],
    declarations: [
        HeaderComponent,
        FooterComponent,
        ShowPicturesComponent,
        NotificationsComponent,
        LoginComponent,
        RegisterComponent,
        RegisterExternalComponent,
        TwoFactorComponent,
        PicturePopupComponent,
        PictureMatchesComponent,
        ShowPictureComponent,
        SelectCollectionComponent,
        ApprovalComponent,
    ],
    exports: [
        TranslateModule,
        PipesModule,
        DirectivesModule,
        LazyLoadImageModule,
        HeaderComponent,
        FooterComponent,
        ShowPicturesComponent,
        NotificationsComponent,
        LoginComponent,
        RegisterComponent,
        RegisterExternalComponent,
        TwoFactorComponent,
        PictureMatchesComponent,
        ShowPictureComponent,
        ApprovalComponent,
    ],
})
export class ComponentsModule {}
