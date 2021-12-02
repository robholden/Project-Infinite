import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { TranslateModule } from '@ngx-translate/core';

import { ComponentsModule } from '@app/components/component.module';
import { DirectivesModule } from '@app/directives/directives.module';
import { FeaturesModule } from '@app/features/features.module';

import { PipesModule } from '@shared/pipes/pipes.module';

import { AuthModal } from './auth-modal/auth.modal';
import { ChangeSettingsModal } from './change-settings/change-settings.modal';
import { ModifyCollectionModal } from './modify-collection/modify-collection.modal';
import { PictureEditModal } from './picture-edit/picture-edit.modal';
import { PictureUploadModal } from './picture-upload/picture-upload.modal';
import { ReportPictureModal } from './report-picture/report-picture.modal';

import { LazyLoadImageModule } from 'ng-lazyload-image';
import { MarkdownModule } from 'ngx-markdown';
import { SortablejsModule } from 'ngx-sortablejs';

@NgModule({
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        ComponentsModule,
        FeaturesModule,
        LazyLoadImageModule,
        PipesModule,
        DirectivesModule,
        MarkdownModule.forRoot(),
        SortablejsModule,
    ],
    declarations: [PictureUploadModal, PictureEditModal, ModifyCollectionModal, ChangeSettingsModal, AuthModal, ReportPictureModal],
    exports: [TranslateModule, PipesModule, DirectivesModule, LazyLoadImageModule],
})
export class ModalsModule {}
