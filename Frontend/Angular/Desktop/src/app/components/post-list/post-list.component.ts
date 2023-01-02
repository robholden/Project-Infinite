import { Component, ContentChild, Directive, OnInit, TemplateRef } from '@angular/core';

import { enumValues } from '@shared/functions';
import { PageRequest, Post, PostOrderByEnum, PostSearch } from '@shared/models';
import { PostService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

import { PagedComponent } from '@app/features/paged-component.helper';
import { PostEditModal } from '@app/modals/post-edit-modal/post-edit.modal';
import { ModalController } from '@app/shared/controllers/modal';

@Directive({ selector: '[posts-header]' })
export class PostsHeaderDirective {
    constructor(public template: TemplateRef<any>) {}
}

@Component({
    selector: 'pi-post-list',
    templateUrl: './post-list.component.html',
    styleUrls: ['./post-list.component.scss'],
})
export class PostListComponent extends PagedComponent<Post, PostSearch> implements OnInit {
    @ContentChild(PostsHeaderDirective, { read: TemplateRef, static: true }) content?: TemplateRef<any>;

    readonly orderByValues = enumValues(PostOrderByEnum, PostOrderByEnum.None);

    get context() {
        return {
            pending: this.getting,
            total: this.result?.totalRows || 0,
        };
    }

    constructor(service: PostService, public authState: AuthState, private modalCtrl: ModalController) {
        super(service, {
            pager: new PageRequest(),
            def: {},
            filterParams: [],
        });
    }

    ngOnInit(): void {}

    async create() {
        const modal = this.modalCtrl.add('create-post', PostEditModal);
        const updated = await modal.present();

        if (updated) {
            this.result.rows.unshift(updated);
            this.result.totalRows++;
        }
    }

    updated(index: number, post: Post) {
        // Post has been deleted
        if (!post) {
            this.result.rows.splice(index, 1);
            this.result.totalRows--;
            return;
        }

        this.result.rows[index] = post;
    }
}
