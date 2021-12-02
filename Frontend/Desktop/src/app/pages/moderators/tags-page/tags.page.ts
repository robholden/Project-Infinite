import { Component, OnInit } from '@angular/core';

import { LoadingController } from '@app/shared/controllers/loading';

import { deepCopy } from '@shared/functions';
import { CustomError, Tag } from '@shared/models';
import { TagService } from '@shared/services/content';

@Component({
    selector: 'sc-tags',
    templateUrl: './tags.page.html',
    styleUrls: ['./tags.page.scss'],
})
export class TagsPage implements OnInit {
    getting = true;
    tags: Tag[];

    private cache: Tag[] = [];

    get has_changes() {
        return this.to_add.length || this.to_delete.length || this.to_update.length;
    }

    get valid_changes() {
        return !this.to_add.some((t) => !t.value);
    }

    get to_add() {
        return this.tags.filter((t) => !t.id);
    }

    get to_update() {
        return this.tags.filter((t) => !!t.id).filter((t) => this.cache.some((ct) => ct.id === t.id && (ct.value !== t.value || ct.weight !== t.weight)));
    }

    get to_delete() {
        return this.cache.filter((t) => !this.tags.some((ct) => ct.id === t.id));
    }

    constructor(private service: TagService, private loadingCtrl: LoadingController) {}

    ngOnInit(): void {
        this.fetch();
    }

    async fetch() {
        this.getting = true;
        const resp = await this.service.getAll();
        this.getting = false;

        if (resp instanceof CustomError) return;
        this.tags = resp;
        this.cache = deepCopy(resp);
    }

    add() {
        this.tags.unshift(new Tag());
        setTimeout(() => document.getElementById('tag-0').focus());
    }

    delete(index: number) {
        this.tags.splice(index, 1);
    }

    reset() {
        this.tags = deepCopy(this.cache);
    }

    async save() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('btn-save-tags');
        loading.present();

        // Save to api
        await this.service.save(this.to_add, this.to_update, this.to_delete);
        await this.fetch();

        // Hide loading
        loading.dismiss();
    }
}
