import { Component, OnInit } from '@angular/core';

import { PageRequest, Post, PostSearch } from '@shared/models';
import { PostService } from '@shared/services/content';

import { PagedComponent } from '@app/features/paged-component.helper';
import { ModalController } from '@app/shared/controllers/modal';

@Component({
    selector: 'pi-post-list',
    templateUrl: './post-list.component.html',
    styleUrls: ['./post-list.component.scss'],
})
export class PostListComponent extends PagedComponent<Post, PostSearch> implements OnInit {
    constructor(service: PostService, private modalCtrl: ModalController) {
        super(service, {
            pager: new PageRequest(),
            def: {},
            filterParams: [],
        });
    }

    ngOnInit(): void {}
}
