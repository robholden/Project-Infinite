import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ToastController } from '@app/shared/controllers/toast';

import { CustomError, Picture } from '@shared/models';
import { PictureModService } from '@shared/services/content';

@Component({
    selector: 'sc-approval',
    templateUrl: './approval.component.html',
    styleUrls: ['./approval.component.scss'],
})
export class ApprovalComponent implements OnInit {
    @Input() picture: Picture;
    @Input() redirect: boolean;
    @Output() acted = new EventEmitter<boolean>();

    constructor(
        private router: Router,
        private service: PictureModService,
        private alertCtrl: AlertController,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {}

    ngOnInit(): void {}

    async act(picture: Picture, approved: boolean) {
        let notes = '';

        if (!approved) {
            const result = await this.alertCtrl.create({
                title: 'Reject Picture',
                message: `Please supply the reason for rejection`,
                inputs: [
                    {
                        label: 'Inappropriate',
                        name: 'reason',
                        type: 'radio',
                        value: '0',
                    },
                    {
                        label: 'Duplicate',
                        name: 'reason',
                        type: 'radio',
                        value: '1',
                    },
                    {
                        label: 'Unrelated',
                        name: 'reason',
                        type: 'radio',
                        value: '2',
                    },
                    {
                        label: 'Other',
                        name: 'reason',
                        type: 'radio',
                        value: '-1',
                    },
                    {
                        label: 'Other',
                        name: 'notes',
                        type: 'textarea',
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

            notes = result.reason === '-1' ? result.notes : result.reason;
        }

        const loading = this.loadingCtrl.addBtn('mod-confirm-btn');
        loading.present();

        const resp = await this.service.action(picture.pictureId, approved, notes);

        loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        this.toastCtrl.add(`Picture has been ${approved ? 'approved' : 'rejected'}`, 'success').present(2500);

        if (this.acted) this.acted.emit(true);

        if (this.redirect) {
            const next = await this.service.next();
            const url = next instanceof CustomError || !next ? '/mod' : `/picture/${next}`;
            this.router.navigate([url]);
        }
    }
}
