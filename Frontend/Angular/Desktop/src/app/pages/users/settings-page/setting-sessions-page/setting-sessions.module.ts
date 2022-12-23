import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { SettingSessionsPage } from './setting-sessions.page';

const routes: Routes = [
    {
        path: '',
        component: SettingSessionsPage,
        data: { title: 'Sessions' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [SettingSessionsPage],
})
export class SettingSessionsPageModule {}
