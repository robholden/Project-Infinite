import { Component, ContentChild, OnInit, TemplateRef } from '@angular/core';

import { PageRequest, Post, PostOrderByEnum, PostSearch } from '@shared/models';
import { PostService } from '@shared/services/content';

import { PagedComponent } from '@app/features/paged-component.helper';
import { ModalController } from '@app/shared/controllers/modal';
import { enumValues } from '@shared/functions';

import { Directive } from '@angular/core';

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

    constructor(service: PostService, private modalCtrl: ModalController) {
        super(service, {
            pager: new PageRequest(),
            def: {},
            filterParams: [],
        });
    }

    ngOnInit(): void {}
}
