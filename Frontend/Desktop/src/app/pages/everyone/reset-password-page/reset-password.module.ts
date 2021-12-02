import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { ResetPasswordPage } from './reset-password.page';

const routes: Routes = [
    {
        path: '',
        data: { title: 'Reset Password' },
        component: ResetPasswordPage,
    },
    {
        path: ':key',
        data: { title: 'Reset Password' },
        component: ResetPasswordPage,
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, FormsModule, RouterModule.forChild(routes)],
    declarations: [ResetPasswordPage],
})
export class ResetPasswordPageModule {}
