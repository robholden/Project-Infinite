import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { CustomError, Post } from '@shared/models';
import { PostService } from '@shared/services/content';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ToastController } from '@app/shared/controllers/toast';
import { DesktopStore } from '@app/storage/desktop.store';

@Component({
    selector: 'pi-post-edit',
    templateUrl: './post-edit.component.html',
    styleUrls: ['./post-edit.component.scss'],
})
export class PostEditComponent implements OnInit, OnDestroy {
    @Input() post: Post;
    @Output() postChange = new EventEmitter<Post>();
    @Output() closed = new EventEmitter<void>();

    form: FormGroup<{
        title: FormControl<string>;
        body: FormControl<string>;
    }>;

    constructor(
        private postService: PostService,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController,
        private alertCtrl: AlertController,
        private desktopStore: DesktopStore
    ) {}

    ngOnInit(): void {
        this.form = new FormGroup({
            title: new FormControl(this.post?.title || '', [Validators.required, Validators.minLength(4), Validators.maxLength(100)]),
            body: new FormControl(this.post?.body || '', [Validators.required, Validators.minLength(4), Validators.maxLength(500)]),
        });

        this.desktopStore.setCanReload({ fn: () => false });
    }

    ngOnDestroy(): void {
        this.desktopStore.unsetCanReload();
    }

    async update() {
        const loading = this.loadingCtrl.addBtn('save-btn');
        loading.present();
        this.form.disable();

        const post: Partial<Post> = {
            ...(this.post || {}),
            title: this.form.value.title,
            body: this.form.value.body,
        };
        const resp = this.post ? await this.postService.update(this.post.postId, post) : await this.postService.create(post);

        loading.dismiss();
        this.form.enable();

        if (resp instanceof CustomError) {
            return;
        }

        this.toastCtrl.add('Post saved successfully', 'primary').present(2500);
        this.postChange.emit(resp);
    }

    async delete() {
        await this.alertCtrl.confirm({
            title: 'Delete Post',
            message: 'Are you sure you want to delete this post?',
            confirmBtn: {
                text: 'Delete',
                colour: 'danger',
            },
            focusFirst: true,
            dismissWhen: async (confirmed: boolean) => {
                if (!confirmed) return;

                const loading = this.loadingCtrl.addBtn('delete-btn');
                loading.present();
                this.form.disable();

                const resp = await this.postService.delete(this.post.postId);

                loading.dismiss();
                this.form.enable();

                if (!resp) {
                    return false;
                }

                this.toastCtrl.add('Post deleted successfully', 'primary').present(2500);
                this.postChange.emit(null);
                return true;
            },
        });
    }

    dismiss() {
        this.closed.emit();
    }
}
