import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { PolicyCookiesPage } from './policy-cookies.page';

const routes: Routes = [
    {
        path: '',
        component: PolicyCookiesPage,
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [PolicyCookiesPage]
})
export class PolicyCookiesPageModule {}
