import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

import { promptForPassword } from '@app/functions/password-prompt.fn';

import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { AlertController } from '@ionic/angular';
import { Storage } from '@ionic/storage-angular';

import { CustomError, PasswordRequest, SMap, UserField, userValidators } from '@shared/models';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'pi-update-user-field',
    templateUrl: './update-user-field.component.html',
    styleUrls: ['./update-user-field.component.scss'],
})
export class UpdateUserFieldComponent implements OnInit {
    form: FormGroup;

    errors: string[] = [];
    trxParams: SMap<any> = {};

    @Input() requestPw: boolean;
    @Input() readonly: boolean;
    @Input() type: string = 'text';
    @Input() label: string;
    @Input() field: UserField;
    @Output() saved = new EventEmitter<string>();

    get value() {
        return this.form.get('value');
    }

    get hasChange() {
        return this.value.value !== this.authState.user[this.field];
    }

    get valid() {
        return this.form && !this.form.invalid && this.hasChange;
    }

    constructor(
        private authState: AuthState,
        private userService: UserService,
        private fb: FormBuilder,
        private alertCtrl: AlertController,
        private fingerprint: FingerprintAIO,
        private storage: Storage
    ) {}

    ngOnInit() {
        this.setForm();
    }

    async save() {
        if (!this.valid) return;

        let passwordRequest: PasswordRequest;
        if (this.requestPw) {
            const passwordRequest = await promptForPassword(
                {
                    touchId: { message: 'Scan your fingerprint to make this change' },
                    password: { message: 'Your password is required to make this change', submitText: 'Save' },
                },
                this.alertCtrl,
                this.fingerprint,
                this.storage,
                this.authState
            );
            if (passwordRequest === null) return;
        }

        const resp = this.userService.update(this.field, this.value.value, passwordRequest);
        if (resp instanceof CustomError) return;

        this.saved.emit(this.value.value);
    }

    private setForm() {
        this.form = this.fb.group({
            value: [this.authState.user[this.field], userValidators(this.field)],
        });

        switch (this.field) {
            case 'name':
                this.errors = ['maxlength'];
                break;

            case 'email':
                this.errors = ['maxlength', 'email'];
                break;

            case 'username':
                this.errors = ['minlength', 'maxlength', 'pattern'];
                this.trxParams = { pattern: 'username' };
                break;
        }
    }
}
