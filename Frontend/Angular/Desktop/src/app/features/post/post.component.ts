import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import { Post } from '@shared/models';
import { AuthState } from '@shared/storage';

import { PostEditModal } from '@app/modals/post-edit-modal/post-edit.modal';
import { ModalController } from '@app/shared/controllers/modal';

@Component({
    selector: 'pi-post',
    templateUrl: './post.component.html',
    styleUrls: ['./post.component.scss'],
})
export class PostComponent implements OnInit {
    @Input() post: Post;
    @Output() postChange = new EventEmitter<Post>();

    author: boolean;

    constructor(private authState: AuthState, private modalCtrl: ModalController) {}

    ngOnInit(): void {
        this.author = this.authState.is_admin || (this.authState.loggedIn && this.authState.snapshot('user')?.username === this.post?.author);
    }

    async edit() {
        const modal = this.modalCtrl.add('edit-post', PostEditModal, { post: this.post });
        const updated = await modal.present();

        if (updated || updated === false) this.postChange.emit(updated ? updated : null);
    }
}
