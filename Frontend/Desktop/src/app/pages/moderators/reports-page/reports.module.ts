import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { ActionPictureReportModal } from './picture-reports-page/action-picture-report/action-picture-report.modal';
import { PictureReportsPage } from './picture-reports-page/picture-reports.page';
import { ActionUserReportModal } from './user-reports-page/action-user-report/action-user-report.modal';
import { UserReportsPage } from './user-reports-page/user-reports.page';

const routes: Routes = [
    {
        path: '',
        component: UserReportsPage,
        data: { title: 'User Reports' },
    },
    {
        path: 'pictures',
        component: PictureReportsPage,
        data: { title: 'Picture Reports' },
    },
];

@NgModule({
    imports: [SharedModule, FormsModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [UserReportsPage, PictureReportsPage, ActionUserReportModal, ActionPictureReportModal],
})
export class ReportsPageModule {}
