import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

import { CustomError, ReportUserReason, User } from '@shared/models';
import { UserService } from '@shared/services/identity';

@Component({
    selector: 'sc-report-user',
    templateUrl: './report-user.modal.html',
    styleUrls: ['./report-user.modal.scss'],
})
export class ReportUserModal extends ModalComponent<boolean> implements OnInit {
    @Input() user: User;

    form: FormGroup;

    ReportReason = ReportUserReason;

    constructor(
        injector: Injector,
        private fb: FormBuilder,
        private service: UserService,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.form = this.fb.group({
            reason: [null, [Validators.required]],
        });
    }

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
