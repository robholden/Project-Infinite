import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { ModeratorPage } from './moderator.page';

const routes: Routes = [
    {
        path: '',
        component: ModeratorPage,
        data: { title: 'Mod' },
        children: [
            { path: '', redirectTo: 'reports', pathMatch: 'full' },
            { path: 'reports', loadChildren: () => import('./reports-page/reports.module').then((m) => m.ReportsPageModule) },
        ],
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [ModeratorPage],
})
export class ModeratorPageModule {}
