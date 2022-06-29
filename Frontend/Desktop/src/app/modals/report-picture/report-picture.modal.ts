import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { CustomError, Picture, ReportPictureReason } from '@shared/models';
import { PictureService } from '@shared/services/content';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

@Component({
    selector: 'sc-report-picture',
    templateUrl: './report-picture.modal.html',
    styleUrls: ['./report-picture.modal.scss'],
})
export class ReportPictureModal extends ModalComponent<boolean> implements OnInit {
    @Input() picture: Picture;

    form = new FormGroup({
        reason: new FormControl<ReportPictureReason>(null, [Validators.required]),
    });

    ReportReason = ReportPictureReason;

    constructor(injector: Injector, private service: PictureService, private loadingCtrl: LoadingController, private toastCtrl: ToastController) {
        super(injector);
    }

    ngOnInit(): void {}

    async submit() {
        const loading = this.loadingCtrl.addBtn('submit-report-btn');
        loading.present();

        const resp = this.service.report(this.picture.pictureId, this.form.value.reason);

        loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        this.toastCtrl.add('A report has been submitted', 'success').present(5000);
        this.result.next(true);
    }
}
