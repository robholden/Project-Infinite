import { Component, Injector, Input, OnInit } from '@angular/core';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

import { CustomError, ReportedAction, ReportUserReason, User, UserReport } from '@shared/models';
import { UserService } from '@shared/services/identity';
import { ReportService } from '@shared/services/reports';

@Component({
    selector: 'pi-action-user-report',
    templateUrl: './action-user-report.modal.html',
    styleUrls: ['./action-user-report.modal.scss'],
})
export class ActionUserReportModal extends ModalComponent<boolean> implements OnInit {
    @Input() report: UserReport;

    executing: boolean;
    user: User;

    ReportReason = ReportUserReason;

    constructor(
        injector: Injector,
        private reportService: ReportService<UserReport>,
        private userService: UserService,
        private alertCtrl: AlertController,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.fetchUser();
    }

    ignoreReport() {
        this.submit(ReportedAction.Ignore);
    }

    confirmReport() {
        this.submit(ReportedAction.Confirm);
    }

    private async submit(action: ReportedAction) {
        let notes = '';
        let sendEmail = false;

        if (action === ReportedAction.Confirm) {
            const result = await this.alertCtrl.create({
                title: 'Confirm Report',
                message: `Your action will permanently delete this user. Are you sure?`,
                inputs: [
                    {
                        label: 'Notes',
                        name: 'notes',
                        type: 'textarea',
                    },
                    {
                        label: 'Send email to user',
                        name: 'sendEmail',
                        type: 'checkbox',
                    },
                ],
                buttons: [
                    {
                        text: 'Cancel',
                        role: 'cancel',
                        className: 'mr-a',
                    },
                    {
                        text: 'Confirm Report',
                        role: 'submit',
                        colour: 'danger',
                        className: 'ml-a',
                    },
                ],
            });
            if (!result) return;

            notes = result.notes;
            sendEmail = result.email;
        }

        const loading = this.loadingCtrl.addBtn((action === ReportedAction.Ignore ? 'ignore' : 'confirm') + '-report-btn');
        loading.present();
        this.executing = true;
        this.preventClosing(true);

        const resp = await this.reportService.actionUser(this.report.reportId, action, notes, sendEmail);

        loading.dismiss();
        this.executing = false;
        this.preventClosing(false);

        if (resp instanceof CustomError) {
            return;
        }

        const msg = 'Report completed and closed, thanks!';
        this.toastCtrl.add(msg, 'success').present(2500);
        this.result.next(true);
    }

    private async fetchUser() {
        const resp = await this.userService.get(this.report.username);
        if (resp instanceof CustomError) {
            return;
        }

        this.user = resp;
    }
}
