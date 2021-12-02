import { Component, HostListener, Injector, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';
import { DesktopStore } from '@app/storage/desktop.store';

import { CustomEvent } from '@shared/enums';
import { base64FileSizeInMb, deepCopy, validateFileGeoLocation } from '@shared/functions';
import { CustomError, ErrorCode, Picture } from '@shared/models';
import { EventService } from '@shared/services';
import { PictureService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

class FileInstance {
    url: string = '';
    file: File = null;
    progress: number = -1;
    errors: CustomError[] = [];

    constructor(public id: number) {}
}

@Component({
    selector: 'sc-picture-upload',
    templateUrl: './picture-upload.modal.html',
    styleUrls: ['./picture-upload.modal.scss'],
})
export class PictureUploadModal extends ModalComponent<any> implements OnInit {
    loading = true;
    uploads: FileInstance[] = [0, 1, 2, 3, 4, 5].map((id) => new FileInstance(id));
    drop_index: number = null;
    completed: boolean;

    get files(): FileInstance[] {
        return this.uploads.filter((upload) => !!upload.file);
    }

    get has_file(): boolean {
        return this.files.length > 0;
    }

    get has_errors(): boolean {
        return this.uploads.some((upload) => upload.errors.length > 0);
    }

    get progress(): number {
        const uploading = this.uploads.filter((upload) => upload.progress >= 0);
        return uploading.length === 0 ? -1 : uploading.map((upload) => upload.progress).reduce((a, b) => a + b, 0);
    }

    constructor(
        injector: Injector,
        private router: Router,
        public authState: AuthState,
        private pictureService: PictureService,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController,
        private events: EventService
    ) {
        super(injector);
    }

    ngOnInit(): void {}

    change(file: any, index: number) {
        if (!file.target.files || !file.target.files.length) return;

        this.fillUploads(file.target.files, index);
    }

    async upload() {
        const loading = this.loadingCtrl.addBtn('upload-btn');
        loading.present();
        this.preventClosing(true);

        const stopLoading = () => {
            loading.dismiss();
            this.preventClosing(false);
        };

        const to_upload = this.uploads.filter((upload) => upload.file);
        const verify = await this.pictureService.canUpload(to_upload.length);
        if (verify instanceof CustomError) {
            stopLoading();
            return;
        }

        const results: Array<false | Picture> = await Promise.all(to_upload.map((upload) => this.uploadInstance(upload)));

        stopLoading();
        this.completed = true;

        if (this.has_errors || results.some((r) => typeof r === 'boolean')) {
            return;
        }

        let text = `Picture uploaded successfully`;

        if (results.length === 1 && typeof results[0] === 'object') {
            this.router.navigate([`/picture/${results[0].pictureId}`]);
        } else {
            this.router.navigate([`/user/${this.authState.user.username}/drafts`]);
            text = `${results.length} pictures uploaded successfully`;
        }

        this.events.trigger(CustomEvent.Uploaded);

        // Update our stats
        if (this.authState.userStats) {
            this.authState.userStats.drafts += results.length;
        }

        const toast = this.toastCtrl.add(text);
        toast.present(5000);

        this.dismiss();
    }

    private async uploadInstance(instance: FileInstance) {
        // Talk to our api and log them in
        const resp = await this.pictureService.upload(instance.file, (n) => (instance.progress = n));

        instance.errors = [];
        instance.progress = -1;

        if (resp instanceof CustomError) {
            return false;
        }

        if (resp.errors || !resp.picture) {
            instance.errors = resp.errors.map((e) => new CustomError(e.code, e.params, e.message));
            return false;
        }

        instance.file = null;
        instance.url = null;

        return resp.picture;
    }

    @HostListener('dragover', ['$event'])
    dragover(evt) {
        evt.preventDefault();
        evt.stopPropagation();

        this.drop_index = this.nextAvailableUploadIndex();
    }

    @HostListener('dragleave', ['$event'])
    dragleave(evt) {
        evt.preventDefault();
        evt.stopPropagation();

        this.drop_index = null;
    }

    @HostListener('drop', ['$event'])
    drop(evt) {
        evt.preventDefault();
        evt.stopPropagation();

        const files = evt.dataTransfer.files;
        if (!files || !files.length) return;

        this.fillUploads(files);
    }

    private fillUploads(files: File[], startIndex: number = null) {
        const file_array: File[] = [];
        const len = files.length < this.uploads.length ? files.length : this.uploads.length;
        for (let index = 0; index <= len; index++) {
            file_array.push(files[index]);
        }

        if (file_array.length === 1) {
            this.loadfile(files[0], this.drop_index);
        } else {
            const sorted = deepCopy(this.uploads).sort((a, b) => (a.id === startIndex ? -10 : (a.file ? 1 : 0) - (b.file ? 1 : 0)));
            const slots = sorted.map((u) => this.uploads.findIndex((f) => f.id === u.id));

            for (let index = 0; index <= file_array.length; index++) {
                this.loadfile(file_array[index], slots[index]);
            }
        }

        this.drop_index = null;
    }

    private nextAvailableUploadIndex() {
        const index = this.uploads.findIndex((upload) => !upload.file);
        return index >= 0 ? index : this.uploads.length - 1;
    }

    private loadfile(file: File, index: number) {
        if (!file) return;

        const upload = this.uploads[index];
        if (!upload) return;

        const reader = new FileReader();

        reader.onload = (event: any) => {
            const result = <string>event.target.result;
            const exif_error = validateFileGeoLocation(result);
            const file_size = base64FileSizeInMb(result);

            const errors: CustomError[] = [];

            // TODO: UNDO FOR GEO
            // if (exif_error) errors.push(new CustomError(exif_error));
            if (file_size > 10) errors.push(new CustomError(ErrorCode.MissingExifLocation));

            upload.errors = errors;
            upload.url = result;
            upload.file = file;

            (<any>document.getElementById(`upload-input-${upload.id}`)).value = '';
        };

        reader.readAsDataURL(file);
    }
}
