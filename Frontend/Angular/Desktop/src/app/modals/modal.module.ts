import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { TranslateModule } from '@ngx-translate/core';

import { PipesModule } from '@shared/pipes/pipes.module';

import { ComponentsModule } from '@app/components/component.module';
import { DirectivesModule } from '@app/directives/directives.module';
import { FeaturesModule } from '@app/features/features.module';

import { AuthModal } from './auth-modal/auth.modal';

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
    declarations: [AuthModal],
    exports: [TranslateModule, PipesModule, DirectivesModule, LazyLoadImageModule],
})
export class ModalsModule {}
