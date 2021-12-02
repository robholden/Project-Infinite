import { Component, Injector, Input, OnInit } from '@angular/core';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

import { CustomError, Picture, PictureReport, ReportedAction, ReportPictureReason, User } from '@shared/models';
import { PictureService } from '@shared/services/content';
import { UserService } from '@shared/services/identity';
import { ReportService } from '@shared/services/reports';

@Component({
    selector: 'sc-action-picture-report',
    templateUrl: './action-picture-report.modal.html',
    styleUrls: ['./action-picture-report.modal.scss'],
})
export class ActionPictureReportModal extends ModalComponent<boolean> implements OnInit {
    @Input() report: PictureReport;

    executing: boolean;
    user: User;
    picture: Picture;

    ReportReason = ReportPictureReason;

    constructor(
        injector: Injector,
        private reportService: ReportService<PictureReport>,
        private userService: UserService,
        private pictureService: PictureService,
        private alertCtrl: AlertController,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.fetchUser();
        this.fetchPicture();
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

        const resp = await this.reportService.actionPicture(this.report.reportId, action, notes, sendEmail);

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

    private async fetchPicture() {
        const resp = await this.pictureService.get(this.report.pictureId);
        if (resp instanceof CustomError) {
            return;
        }

        this.picture = resp;
    }
}
