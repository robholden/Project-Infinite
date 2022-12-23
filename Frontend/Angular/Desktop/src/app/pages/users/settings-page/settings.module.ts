import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { SettingsPage } from './settings.page';

const routes: Routes = [
    {
        path: '',
        component: SettingsPage,
        data: { title: 'Settings' },
        children: [
            { path: '', redirectTo: 'details', pathMatch: 'full' },
            { path: 'details', loadChildren: () => import('./setting-details-page/setting-details.module').then((m) => m.SettingDetailsPageModule) },
            { path: 'security', loadChildren: () => import('./setting-security-page/setting-security.module').then((m) => m.SettingSecurityPageModule) },
            {
                path: 'preferences',
                loadChildren: () => import('./setting-preferences-page/setting-preferences.module').then((m) => m.SettingPreferencesPageModule),
            },
            { path: 'sessions', loadChildren: () => import('./setting-sessions-page/setting-sessions.module').then((m) => m.SettingSessionsPageModule) },
        ],
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [SettingsPage]
})
export class SettingsPageModule {}
