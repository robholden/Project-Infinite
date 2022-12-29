import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { CustomError, ReportUserReason, User } from '@shared/models';
import { UserService } from '@shared/services/identity';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

@Component({
    selector: 'pi-report-user',
    templateUrl: './report-user.modal.html',
    styleUrls: ['./report-user.modal.scss'],
})
export class ReportUserModal extends ModalComponent<boolean> implements OnInit {
    @Input() user: User;

    form = new FormGroup({
        reason: new FormControl<ReportUserReason>(null, [Validators.required]),
    });

    ReportReason = ReportUserReason;

    constructor(injector: Injector, private service: UserService, private loadingCtrl: LoadingController, private toastCtrl: ToastController) {
        super(injector);
    }

    ngOnInit(): void {}

    async submit() {
        const loading = this.loadingCtrl.addBtn('submit-report-btn');
        loading.present();

        const resp = this.service.report(this.user.username, this.form.value.reason);

        loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        this.toastCtrl.add('A report has been submitted', 'success').present(5000);
        this.result.next(true);
    }
}
