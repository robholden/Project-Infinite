import { Component, EventEmitter, Injector, Input, Output } from '@angular/core';

import { PictureStatus } from '@shared/enums';
import { CustomError, Picture } from '@shared/models';
import { PictureService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

import { fade } from '@app/functions/animations.fn';
import { deletePicture } from '@app/functions/delete-picture.fn';
import { PictureEditModal } from '@app/modals/picture-edit/picture-edit.modal';
import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ModalController } from '@app/shared/controllers/modal';

import { ReportPictureModal } from '../../modals/report-picture/report-picture.modal';

@Component({
    selector: 'sc-show-picture',
    templateUrl: './show-picture.component.html',
    styleUrls: ['./show-picture.component.scss'],
    animations: [fade],
})
export class ShowPictureComponent {
    @Input() dedicated: boolean;
    @Input() redirectApproval: boolean;

    @Input() picture: Picture;
    @Output() deleted = new EventEmitter();

    Status = PictureStatus;

    nearby: Picture[];
    reported: boolean;
    liking: boolean;
    fetching_tags: boolean;

    get published() {
        return this.picture.status === undefined || this.picture.status === PictureStatus.Published;
    }

    get author() {
        return this.authState.user && this.authState.user.username === this.picture.username;
    }

    constructor(
        private injector: Injector,
        public authState: AuthState,
        private pictureService: PictureService,
        private modalCtrl: ModalController,
        private loadingCtrl: LoadingController,
        private alertCtrl: AlertController
    ) {}

    async edit() {
        const modal = this.modalCtrl.add('edit-picture', PictureEditModal, { pictureId: this.picture.pictureId });
        const updated = await modal.present();
        if (updated) this.picture = updated;
        else if (updated === false) {
            this.picture = null;
            this.deleted.emit();
        }
    }

    async like() {
        this.liking = true;
        const resp = await this.pictureService.like(this.picture.pictureId, !this.picture.liked);
        this.liking = false;

        if (resp instanceof CustomError) return;

        this.picture.likesTotal += this.picture.liked ? -1 : 1;
        this.picture.liked = !this.picture.liked;
    }

    async report() {
        if (this.reported) return;

        const modal = this.modalCtrl.add<boolean>('report-picture', ReportPictureModal, { picture: this.picture });
        this.reported = await modal.present();
    }

    async publish() {
        const result = await this.alertCtrl.confirm({
            title: 'Submit for approval',
            message: 'Are you sure you want to submit this picture? Before it goes live, it will be reviewed by our moderation team',
            confirmBtn: { text: 'Publish' },
        });
        if (!result) return;

        const loading = this.loadingCtrl.addBtn('submit-btn');
        loading.present();

        const resp = await this.pictureService.submit(this.picture.pictureId);

        loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        this.picture.status = PictureStatus.PendingApproval;
    }

    async delete() {
        const didDelete = await deletePicture(this.injector, this.picture);
        if (didDelete) {
            this.picture = null;
            this.deleted.emit();
        }
    }
}
