import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

import { Collection, CustomError } from '@shared/models';
import { CollectionService } from '@shared/services/content';

@Component({
    selector: 'sc-modify-collection',
    templateUrl: './modify-collection.modal.html',
    styleUrls: ['./modify-collection.modal.scss'],
})
export class ModifyCollectionModal extends ModalComponent<Collection> implements OnInit {
    @Input() collection: Collection;

    form: FormGroup;

    constructor(
        injector: Injector,
        private fb: FormBuilder,
        private service: CollectionService,
        private loadingCtrl: LoadingController,
        private toastCtrl: ToastController
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.form = this.fb.group({
            name: [this.collection ? this.collection.name : '', [Validators.required, Validators.minLength(4), Validators.maxLength(100)]],
        });

        setTimeout(() => document.getElementById('collection-name').focus());
    }

    async load() {}

    async save() {
        const loading = this.loadingCtrl.addBtn('save-collection-btn');
        loading.present();
        this.preventClosing(true);

        const updating = this.collection && !!this.collection.collectionId;
        const collection = {
            ...(this.collection || new Collection()),
            name: this.form.get('name').value,
        };

        const resp = updating ? await this.service.update(this.collection.collectionId, collection) : await this.service.create(collection);

        loading.dismiss();
        this.preventClosing(false);

        if (resp instanceof CustomError) {
            return;
        }

        const msg = `Collection ${updating ? 'saved' : 'created'} successfully`;
        this.toastCtrl.add(msg, 'success').present(5000);
        this.result.next(resp);
    }
}
