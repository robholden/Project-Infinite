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

import { MarkdownModule } from 'ngx-markdown';

@NgModule({
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        ComponentsModule,
        FeaturesModule,
        PipesModule,
        DirectivesModule,
        MarkdownModule.forRoot(),
    ],
    declarations: [AuthModal],
    exports: [TranslateModule, PipesModule, DirectivesModule],
})
export class ModalsModule {}
