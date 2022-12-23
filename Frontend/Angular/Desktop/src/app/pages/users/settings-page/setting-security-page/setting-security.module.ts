import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { SettingSecurityPage } from './setting-security.page';
import { SetupTwoFactorModal } from './setup-two-factor-modal/setup-two-factor.modal';

const routes: Routes = [
    {
        path: '',
        component: SettingSecurityPage,
        data: { title: 'Security' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [SettingSecurityPage, SetupTwoFactorModal]
})
export class SettingSecurityPageModule {}
