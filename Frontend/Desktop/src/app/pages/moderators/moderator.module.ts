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
            { path: '', redirectTo: 'approvals', pathMatch: 'full' },
            { path: 'approvals', loadChildren: () => import('./approvals-page/approvals.module').then((m) => m.ApprovalsPageModule) },
            { path: 'reports', loadChildren: () => import('./reports-page/reports.module').then((m) => m.ReportsPageModule) },
            { path: 'locations', loadChildren: () => import('./locations-page/locations.module').then((m) => m.LocationsPageModule) },
            {
                path: 'tags',
                loadChildren: () => import('./tags-page/tags.module').then((m) => m.TagsPageModule),
                data: { roles: ['Admin'] },
            },
        ],
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [ModeratorPage],
    entryComponents: [],
})
export class ModeratorPageModule {}
