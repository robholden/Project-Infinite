import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { PipesModule } from '@shared/pipes/pipes.module';

import { AvatarComponent } from './avatar/avatar.component';
import { CookieConsentComponent } from './cookie-consent/cookie-consent.component';
import { DropDownComponent } from './drop-down/drop-down.component';
import { EllipsisComponent } from './ellipsis/ellipsis.component';
import { ModalWrapperComponent } from './modal-wrapper/modal-wrapper.component';
import { PageContentComponent } from './page-wrapper/page-content/page-content.component';
import { PageHeaderComponent } from './page-wrapper/page-header/page-header.component';
import { PageSectionComponent } from './page-wrapper/page-section/page-section.component';
import { PageWrapperComponent } from './page-wrapper/page-wrapper.component';
import { PagingComponent } from './paging/paging.component';
import { RememberMeComponent } from './remember-me/remember-me.component';
import { SkeletonTextComponent } from './skeleton-text/skeleton-text.component';
import { ToggleSwitchComponent } from './toggle-switch/toggle-switch.component';

@NgModule({
    imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, PipesModule],
    declarations: [
        ToggleSwitchComponent,
        DropDownComponent,
        CookieConsentComponent,
        PageWrapperComponent,
        PageHeaderComponent,
        PageContentComponent,
        PageSectionComponent,
        PagingComponent,
        SkeletonTextComponent,
        ModalWrapperComponent,
        AvatarComponent,
        RememberMeComponent,
        EllipsisComponent,
    ],
    exports: [
        ToggleSwitchComponent,
        DropDownComponent,
        CookieConsentComponent,
        PageWrapperComponent,
        PageHeaderComponent,
        PageContentComponent,
        PageSectionComponent,
        PagingComponent,
        SkeletonTextComponent,
        ModalWrapperComponent,
        AvatarComponent,
        RememberMeComponent,
        EllipsisComponent,
    ],
})
export class FeaturesModule {}
