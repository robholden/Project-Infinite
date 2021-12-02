import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { PolicyTermsPage } from './policy-terms.page';

import { MarkdownModule } from 'ngx-markdown';

const routes: Routes = [
    {
        path: '',
        component: PolicyTermsPage,
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes), MarkdownModule.forRoot()],
    declarations: [PolicyTermsPage],
    entryComponents: [],
})
export class PolicyTermsPageModule {}
