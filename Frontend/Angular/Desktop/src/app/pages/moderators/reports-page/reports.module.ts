import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { ActionUserReportModal } from './user-reports-page/action-user-report/action-user-report.modal';
import { UserReportsPage } from './user-reports-page/user-reports.page';

const routes: Routes = [
    {
        path: '',
        component: UserReportsPage,
        data: { title: 'User Reports' },
    },
];

@NgModule({
    imports: [SharedModule, FormsModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [UserReportsPage, ActionUserReportModal],
})
export class ReportsPageModule {}
