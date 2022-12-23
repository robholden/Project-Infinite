import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { ConfirmEmailPage } from './confirm-email.page';

const routes: Routes = [
    {
        path: ':key',
        component: ConfirmEmailPage,
        data: { title: 'Confirm Email' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [ConfirmEmailPage],
})
export class ConfirmEmailPageModule {}
