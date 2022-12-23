import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { PolicyPrivacyPage } from './policy-privacy.page';

import { MarkdownModule } from 'ngx-markdown';

const routes: Routes = [
    {
        path: '',
        component: PolicyPrivacyPage,
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes), MarkdownModule.forRoot()],
    declarations: [PolicyPrivacyPage]
})
export class PolicyPrivacyPageModule {}
