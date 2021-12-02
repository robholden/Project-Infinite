import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { PoliciesPage } from './policies.page';

const routes: Routes = [
    {
        path: '',
        component: PoliciesPage,
        data: { title: 'Policies' },
        children: [
            {
                path: 'terms',
                data: { title: 'Terms & Conditions' },
                loadChildren: () => import('./policy-terms-page/policy-terms.module').then((m) => m.PolicyTermsPageModule),
            },
            {
                path: 'privacy',
                data: { title: 'Privacy Policy' },
                loadChildren: () => import('./policy-privacy-page/policy-privacy.module').then((m) => m.PolicyPrivacyPageModule),
            },
            {
                path: 'cookies',
                data: { title: 'Cookie Policy' },
                loadChildren: () => import('./policy-cookies-page/policy-cookies.module').then((m) => m.PolicyCookiesPageModule),
            },
            { path: '', redirectTo: 'terms', pathMatch: 'full' },
        ],
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [PoliciesPage],
    entryComponents: [],
})
export class PoliciesPageModule {}
