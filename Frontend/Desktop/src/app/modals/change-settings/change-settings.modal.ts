import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

import { ContentSettings, CustomError } from '@shared/models';
import { AdminService } from '@shared/services/identity';

@Component({
    selector: 'sc-change-settings',
    templateUrl: './change-settings.modal.html',
    styleUrls: ['./change-settings.modal.scss'],
})
export class ChangeSettingsModal extends ModalComponent<boolean> implements OnInit {
    form: FormGroup;

    @Input() userId: string;

    constructor(
        injector: Injector,
        private fb: FormBuilder,
        private service: AdminService,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.load();
    }

    private async load() {
        const settings = await this.service.getUserSettings(this.userId);
        if (settings instanceof CustomError) {
            this.dismiss();
            return;
        }

        this.form = this.fb.group({
            maxPictureSize: [settings.maxPictureSize, [Validators.required]],
            minPictureResolutionX: [settings.minPictureResolutionX, [Validators.required]],
            minPictureResolutionY: [settings.minPictureResolutionY, [Validators.required]],
            uploadLimit: [settings.uploadLimit, [Validators.required]],
            uploadEnabled: [settings.uploadEnabled, []],
        });
    }

    async submit() {
        const loading = this.loadingCtrl.addBtn('save-settings-btn');
        loading.present();

        const settings: ContentSettings = {
            maxPictureSize: this.form.get('maxPictureSize').value,
            minPictureResolutionX: this.form.get('minPictureResolutionX').value,
            minPictureResolutionY: this.form.get('minPictureResolutionY').value,
            uploadLimit: this.form.get('uploadLimit').value,
            uploadEnabled: this.form.get('uploadEnabled').value,
        };
        const resp = await this.service.updateUserSettings(this.userId, settings);

        loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        this.toastCtrl.add('User settings updated successfully', 'success').present(5000);
        this.result.next(true);
    }
}
