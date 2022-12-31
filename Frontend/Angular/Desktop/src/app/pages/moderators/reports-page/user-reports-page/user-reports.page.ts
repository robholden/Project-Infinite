import { Component, OnInit } from '@angular/core';

import { PageRequest, ReportSearch, ReportUserReason, UserReport } from '@shared/models';
import { ReportService } from '@shared/services/reports';

import { PagedComponent } from '@app/features/paged-component.helper';
import { ModalController } from '@app/shared/controllers/modal';

import { ActionUserReportModal } from './action-user-report/action-user-report.modal';

@Component({
    selector: 'pi-user-reports',
    templateUrl: './user-reports.page.html',
    styleUrls: ['./user-reports.page.scss'],
})
export class UserReportsPage extends PagedComponent<UserReport, ReportSearch> implements OnInit {
    readonly ReportReason = ReportUserReason;

    constructor(service: ReportService<UserReport>, private modalCtrl: ModalController) {
        super(
            service,
            {
                pager: new PageRequest(),
                def: { actioned: false },
                filterParams: [],
            },
            { route: '/users' }
        );
    }

    ngOnInit(): void {}

    async action(report: UserReport) {
        const modal = this.modalCtrl.add('report-user', ActionUserReportModal, { report });
        if (await modal.present()) this.reload();
    }
}
