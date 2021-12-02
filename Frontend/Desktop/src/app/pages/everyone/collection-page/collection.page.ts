import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';

import { fade } from '@app/functions/animations.fn';
import { replaceLoadingTitle } from '@app/functions/routing-titles.fn';
import { ModifyCollectionModal } from '@app/modals/modify-collection/modify-collection.modal';
import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ModalController } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';
import { wait } from '@shared/functions';

import { Collection, CustomError, PagedList, Picture } from '@shared/models';
import { CollectionService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'sc-collection-page',
    templateUrl: './collection.page.html',
    styleUrls: ['./collection.page.scss'],
    animations: [fade],
})
export class CollectionPage implements OnInit {
    loading: boolean = true;
    collection: Collection;
    author: boolean;
    result: PagedList<Picture>;

    constructor(
        public authState: AuthState,
        private activatedRoute: ActivatedRoute,
        private title: Title,
        private router: Router,
        private service: CollectionService,
        private loadingCtrl: LoadingController,
        private alertCtrl: AlertController,
        private toastCtrl: ToastController,
        private modalCtrl: ModalController
    ) {
        this.activatedRoute.params.subscribe((params) => this.fetch(params['collectionId']));
    }

    ngOnInit(): void {}

    async fetch(key: string) {
        // Verify the key with the server
        const resp = await this.service.get(key);
        this.loading = false;

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            replaceLoadingTitle(this.title, ':(');
            return;
        }

        replaceLoadingTitle(this.title, resp.name);
        this.collection = resp;
        if (this.authState.user) {
            this.author = this.authState.user && this.authState.user.username === resp.username;
        }
    }

    async edit() {
        const modal = this.modalCtrl.add('edit-collection-modal', ModifyCollectionModal, { collection: this.collection });
        const resp = await modal.present();
        if (!resp) return;

        this.collection = resp;
    }

    async delete() {
        const confirmed = await this.alertCtrl.confirm({
            title: 'Deletion Collection',
            message: 'Are you sure you want to permanently delete this collection?',
            confirmBtn: {
                text: 'Delete',
                colour: 'danger',
            },
            focusFirst: true,
        });
        if (!confirmed) return;

        const loading = this.loadingCtrl.addBtn('delete-btn');
        loading.present();

        const resp = await this.service.delete(this.collection.collectionId);

        loading.dismiss();
        if (resp instanceof CustomError) {
            return;
        }

        this.toastCtrl.add('Collection deleted successfully', 'success').present(5000);
        await this.router.navigateByUrl('/');
    }
}
