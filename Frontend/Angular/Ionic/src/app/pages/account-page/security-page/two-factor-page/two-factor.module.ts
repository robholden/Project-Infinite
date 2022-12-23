import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/components.module';

import { IonicModule } from '@ionic/angular';

import { SetMobileModal } from './modals/set-mobile/set-mobile.modal';
import { SetupTwoFactorModal } from './modals/setup-two-factor/setup-two-factor.modal';
import { TwoFactorPage } from './two-factor.page';

const routes: Routes = [
    {
        path: '',
        component: TwoFactorPage,
    },
];

@NgModule({
    imports: [CommonModule, FormsModule, IonicModule, ReactiveFormsModule, RouterModule.forChild(routes), ComponentsModule],
    declarations: [TwoFactorPage, SetupTwoFactorModal, SetMobileModal],
})
export class TwoFactorPageModule {}
