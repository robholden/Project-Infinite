import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { LoadingTitlePlaceholder } from '@app/functions/routing-titles.fn';
import { ModalsModule } from '@app/modals/modal.module';
import { SharedModule } from '@app/shared/shared.module';

import { AuthGuard } from '@shared/middleware';

import { ReportUserModal } from './report-user/report-user.modal';
import { ShowCollectionsComponent } from './show-collections/show-collections.component';
import { UserPage } from './user.page';

const routes: Routes = [
    {
        path: ':username',
        component: UserPage,
        data: { title: LoadingTitlePlaceholder, action: '' },
    },
    {
        path: ':username/drafts',
        component: UserPage,
        data: { title: LoadingTitlePlaceholder, action: 'drafts' },
        canActivate: [AuthGuard],
    },
    {
        path: ':username/likes',
        component: UserPage,
        data: { title: LoadingTitlePlaceholder, action: 'likes' },
    },
    {
        path: ':username/collections',
        component: UserPage,
        data: { title: LoadingTitlePlaceholder, action: 'collections' },
    },
];

@NgModule({
    imports: [SharedModule, FormsModule, ComponentsModule, ModalsModule, RouterModule.forChild(routes)],
    declarations: [UserPage, ShowCollectionsComponent, ReportUserModal],
})
export class UserPageModule {}
