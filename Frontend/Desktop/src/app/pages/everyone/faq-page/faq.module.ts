import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { FAQPage } from './faq.page';

const routes: Routes = [
    {
        path: '',
        component: FAQPage,
        data: { title: 'FAQs' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [FAQPage]
})
export class FAQPageModule {}
