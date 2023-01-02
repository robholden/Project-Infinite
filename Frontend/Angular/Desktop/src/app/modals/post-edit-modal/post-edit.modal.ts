import { Component, Input, OnInit } from '@angular/core';

import { Post } from '@shared/models';

import { ModalComponent } from '@app/shared/controllers/modal';

@Component({
    selector: 'pi-post-edit-modal',
    templateUrl: './post-edit.modal.html',
    styleUrls: ['./post-edit.modal.scss'],
})
export class PostEditModal extends ModalComponent<Post | false> implements OnInit {
    @Input() post: Post;

    constructor() {
        super();
    }
}
