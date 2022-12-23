import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { SettingPreferencesPage } from './setting-preferences.page';

const routes: Routes = [
    {
        path: '',
        component: SettingPreferencesPage,
        data: { title: 'Preferences' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [SettingPreferencesPage],
})
export class SettingPreferencesPageModule {}
