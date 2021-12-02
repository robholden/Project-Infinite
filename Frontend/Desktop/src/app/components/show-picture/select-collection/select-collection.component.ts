import { Component, Input, OnInit } from '@angular/core';

import { LoadingBtn, LoadingController } from '@app/shared/controllers/loading';

import { CustomEvent } from '@shared/enums';
import { Collection, CollectionSearch, CustomError, PagedList, PageRequest, SMap } from '@shared/models';
import { EventService } from '@shared/services';
import { CollectionService } from '@shared/services/content';

@Component({
    selector: 'sc-select-collection',
    templateUrl: './select-collection.component.html',
    styleUrls: ['./select-collection.component.scss'],
})
export class SelectCollectionComponent implements OnInit {
    pager: PageRequest = { ...new PageRequest(), pageSize: 50 };
    loading: boolean = true;

    collections: Collection[] = [];
    results: PagedList<Collection>;

    loadingMap: SMap<boolean> = {};
    inCollectionMap: SMap<boolean> = {};

    @Input() autoload: boolean;
    @Input() pictureId: string;
    @Input() btnId: string;
    @Input() search: CollectionSearch;

    constructor(private service: CollectionService, private loadingCtrl: LoadingController, private events: EventService) {}

    ngOnInit(): void {
        if (this.autoload) setTimeout(() => this.fetch(), 0);
    }

    async fetch(next: boolean = false) {
        this.loading = !next;
        if (next) this.pager.page++;

        let loading: LoadingBtn;
        if (next) {
            loading = this.loadingCtrl.addBtn('load-more-collection-btn');
            loading.present();
        }

        this.search.includePictureId = this.pictureId;
        const resp = await this.service.lookup(null, this.search, this.pager);

        this.loading = false;
        if (loading) loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        this.results = resp;
        this.collections.push(...resp.rows);
        this.inCollectionMap = this.collections.reduce((acc, curr) => {
            acc[curr.collectionId] = curr.pictures.some((p) => p.pictureId === this.pictureId);
            return acc;
        }, {});
    }

    async openNewCollection() {
        const collection = await this.events.trigger<Collection>(CustomEvent.ModifyCollection);
        if (!collection) return;

        this.collections = [collection];
        this.results.totalRows = 1;
        this.reopen();
    }

    async addRemoveCollection(collectionId: string) {
        this.reopen();

        this.loadingMap[collectionId] = true;
        const resp = await this.service.addRemovePicture(collectionId, this.pictureId);
        this.loadingMap[collectionId] = false;

        if (resp instanceof CustomError) {
            return;
        }

        this.inCollectionMap[collectionId] = !this.inCollectionMap[collectionId];
    }

    private reopen() {
        if (this.btnId) {
            const btn = document.getElementById(this.btnId);
            if (btn) btn.focus();
        }
    }
}
