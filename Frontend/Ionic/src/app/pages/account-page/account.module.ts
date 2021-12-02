import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { ModalsModule } from '@app/modals/modals.module';

import { IonicModule } from '@ionic/angular';

import { AuthGuard } from '@shared/middleware';

import { AccountPage } from './account.page';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        IonicModule,
        RouterModule.forChild([
            {
                path: '',
                component: AccountPage,
            },
            {
                path: 'edit-details',
                loadChildren: () => import('./edit-details-page/edit-details.module').then((m) => m.EditDetailsPageModule),
                canActivateChild: [AuthGuard],
            },
            {
                path: 'security',
                loadChildren: () => import('./security-page/security.module').then((m) => m.SecurityPageModule),
                canActivateChild: [AuthGuard],
            },
            {
                path: 'preferences',
                loadChildren: () => import('./preferences-page/preferences.module').then((m) => m.PreferencesPageModule),
                canActivateChild: [AuthGuard],
            },

            { path: 'feedback', loadChildren: () => import('../feedback-page/feedback.module').then((m) => m.FeedbackPageModule) },
            { path: 'legal', loadChildren: () => import('../legal-page/legal.module').then((m) => m.LegalPageModule) },
        ]),
        ModalsModule,
    ],
    declarations: [AccountPage],
})
export class AccountPageModule {}
