import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { ContentSettings, CustomError } from '@shared/models';
import { AdminService } from '@shared/services/identity';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

@Component({
    selector: 'sc-change-settings',
    templateUrl: './change-settings.modal.html',
    styleUrls: ['./change-settings.modal.scss'],
})
export class ChangeSettingsModal extends ModalComponent<boolean> implements OnInit {
    @Input() userId: string;

    form: FormGroup<{
        maxPictureSize: FormControl<number>;
        minPictureResolutionX: FormControl<number>;
        minPictureResolutionY: FormControl<number>;
        uploadLimit: FormControl<number>;
        uploadEnabled: FormControl<boolean>;
    }>;

    constructor(injector: Injector, private service: AdminService, private loadingCtrl: LoadingController, private toastCtrl: ToastController) {
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

        this.form = new FormGroup({
            maxPictureSize: new FormControl(settings.maxPictureSize, [Validators.required]),
            minPictureResolutionX: new FormControl(settings.minPictureResolutionX, [Validators.required]),
            minPictureResolutionY: new FormControl(settings.minPictureResolutionY, [Validators.required]),
            uploadLimit: new FormControl(settings.uploadLimit, [Validators.required]),
            uploadEnabled: new FormControl(settings.uploadEnabled, []),
        });
    }

    async submit() {
        const loading = this.loadingCtrl.addBtn('save-settings-btn');
        loading.present();

        const settings: ContentSettings = {
            maxPictureSize: this.form.value.maxPictureSize,
            minPictureResolutionX: this.form.value.minPictureResolutionX,
            minPictureResolutionY: this.form.value.minPictureResolutionY,
            uploadLimit: this.form.value.uploadLimit,
            uploadEnabled: this.form.value.uploadEnabled,
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
