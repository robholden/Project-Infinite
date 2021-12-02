import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { SettingDetailsPage } from './setting-details.page';

const routes: Routes = [
    {
        path: '',
        component: SettingDetailsPage,
        data: { title: 'My Details' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [SettingDetailsPage],
})
export class SettingDetailsPageModule {}
