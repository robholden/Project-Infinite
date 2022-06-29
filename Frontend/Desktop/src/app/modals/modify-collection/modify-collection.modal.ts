import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { Collection, CustomError } from '@shared/models';
import { CollectionService } from '@shared/services/content';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

@Component({
    selector: 'sc-modify-collection',
    templateUrl: './modify-collection.modal.html',
    styleUrls: ['./modify-collection.modal.scss'],
})
export class ModifyCollectionModal extends ModalComponent<Collection> implements OnInit {
    @Input() collection: Collection;

    form: FormGroup<{ name: FormControl<string> }>;

    constructor(injector: Injector, private service: CollectionService, private loadingCtrl: LoadingController, private toastCtrl: ToastController) {
        super(injector);
    }

    ngOnInit(): void {
        this.form = new FormGroup({
            name: new FormControl(this.collection ? this.collection.name : '', [Validators.required, Validators.minLength(4), Validators.maxLength(100)]),
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
            name: this.form.value.name,
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
