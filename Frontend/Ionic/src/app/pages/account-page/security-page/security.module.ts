import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/components.module';

import { IonicModule } from '@ionic/angular';

import { AuthGuard } from '@shared/middleware';

import { ChangePasswordModal } from './modals/change-password/change-password.modal';
import { RemoveAccountModal } from './modals/remove-account/remove-account.modal';
import { SecurityPage } from './security.page';

const routes: Routes = [
    {
        path: '',
        component: SecurityPage,
    },
    {
        path: 'sessions',
        loadChildren: () => import('./sessions-page/sessions.module').then((m) => m.SessionsPageModule),
        canActivateChild: [AuthGuard],
    },
    {
        path: 'two-factor',
        loadChildren: () => import('./two-factor-page/two-factor.module').then((m) => m.TwoFactorPageModule),
        canActivateChild: [AuthGuard],
    },
];

@NgModule({
    imports: [CommonModule, FormsModule, ReactiveFormsModule, IonicModule, RouterModule.forChild(routes), ComponentsModule],
    declarations: [SecurityPage, ChangePasswordModal, RemoveAccountModal],
})
export class SecurityPageModule {}
