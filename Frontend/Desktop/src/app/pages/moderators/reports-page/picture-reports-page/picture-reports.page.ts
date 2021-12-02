import { Component, Injector, OnInit } from '@angular/core';

import { PagedComponent } from '@app/features/paged-component.helper';
import { ModalController } from '@app/shared/controllers/modal';

import { PageRequest, PictureReport, ReportPictureReason, ReportSearch } from '@shared/models';
import { ReportService } from '@shared/services/reports';

import { ActionPictureReportModal } from './action-picture-report/action-picture-report.modal';

@Component({
    selector: 'sc-picture-reports',
    templateUrl: './picture-reports.page.html',
    styleUrls: ['./picture-reports.page.scss'],
})
export class PictureReportsPage extends PagedComponent<PictureReport, ReportSearch> implements OnInit {
    ReportReason = ReportPictureReason;

    constructor(injector: Injector, service: ReportService<PictureReport>, private modalCtrl: ModalController) {
        super(
            injector,
            service,
            {
                pager: new PageRequest(),
                def: { actioned: false },
                filterParams: [],
            },
            { route: '/pictures' }
        );
    }

    ngOnInit(): void {}

    async action(report: PictureReport) {
        const modal = this.modalCtrl.add('report-user', ActionPictureReportModal, { report });
        if (await modal.present()) this.reload();
    }
}
