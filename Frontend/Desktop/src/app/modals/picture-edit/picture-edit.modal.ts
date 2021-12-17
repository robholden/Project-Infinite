import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { fade } from '@app/functions/animations.fn';
import { deletePicture } from '@app/functions/delete-picture.fn';
import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

import { waitThen } from '@shared/functions';
import { CustomError, Picture, Tag } from '@shared/models';
import { PictureService, TagService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'sc-picture-edit',
    templateUrl: './picture-edit.modal.html',
    styleUrls: ['./picture-edit.modal.scss'],
    animations: [fade],
})
export class PictureEditModal extends ModalComponent<Picture | false> implements OnInit {
    loading: boolean = true;

    @Input() pictureId: string;
    picture: Picture;
    allTags: Tag[];
    form: FormGroup;

    get name() {
        return this.form.get('name');
    }

    get useRealCoords() {
        return this.form.get('useRealCoords');
    }

    get tags() {
        return this.form.get('tags');
    }

    constructor(
        private injector: Injector,
        private fb: FormBuilder,
        private authState: AuthState,
        private pictureService: PictureService,
        private tagService: TagService,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.fetch();
    }

    addRemoveTag(value: string) {
        const index = this.tags.value.indexOf(value);
        if (index < 0 && this.tags.value.length < 10) this.tags.value.push(value);
        else if (index >= 0) this.tags.value.splice(index, 1);
    }

    async update() {
        const loading = this.loadingCtrl.addBtn('save-btn');
        loading.present();
        this.preventClosing(true);
        this.form.disable();

        const resp = await this.pictureService.update(this.picture.pictureId, {
            name: this.name.value,
            concealCoords: !this.useRealCoords.value,
            tags: this.tags.value,
            seed: this.picture.seed,
        });

        loading.dismiss();
        this.preventClosing(false);
        this.form.enable();

        if (resp instanceof CustomError) {
            return;
        }

        const toast = this.toastCtrl.add('Picture has been updated successfully', 'success');
        toast.present(5000);

        this.result.next(resp);
    }

    async delete() {
        const didDelete = await deletePicture(this.injector, this.picture);
        if (didDelete) this.result.next(false);
    }

    private async fetch() {
        // Get picture and tags
        const [resp, tags] = await Promise.all([this.pictureService.get(this.pictureId, { toastError: true }), this.tagService.getAll()]);
        this.loading = false;

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            this.dismiss();
            return;
        }

        const author = this.authState.user && this.authState.user.username === resp.username;
        if (!author) {
            this.toastCtrl.add('This is not your picture', 'danger').present(2500);
            this.dismiss();
            return;
        }

        if (!(tags instanceof CustomError)) {
            this.allTags = tags;
        }

        this.form = this.fb.group({
            name: [resp.name, [Validators.required, Validators.minLength(4), Validators.maxLength(100)]],
            useRealCoords: [!resp.concealCoords],
            tags: [resp.tags || []],
        });
        this.picture = resp;

        await waitThen(0, () => document.getElementById('pic-name').focus());
    }
}
