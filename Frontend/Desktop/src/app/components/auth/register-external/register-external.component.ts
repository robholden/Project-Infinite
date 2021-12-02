import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent, ModalController } from '@app/shared/controllers/modal';

import { CustomError, ProviderResult, Trx, User, userValidators } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, UserService } from '@shared/services/identity';

@Component({
    selector: 'sc-register-external',
    templateUrl: './register-external.component.html',
    styleUrls: ['./register-external.component.scss'],
})
export class RegisterExternalComponent extends ModalComponent<boolean> implements OnInit {
    @Input() providerResult: ProviderResult;
    @Input() user: User;

    form: FormGroup;
    regError: Trx;
    loginError: Trx;

    constructor(
        injector: Injector,
        public events: EventService,
        private fb: FormBuilder,
        private userService: UserService,
        private authService: AuthService,
        private modalCtrl: ModalController,
        private loadingCtrl: LoadingController
    ) {
        super(injector);
    }

    ngOnInit() {
        this.setForm();
        setTimeout(() => document.getElementById('name').focus(), 0);
    }

    async register() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('register-btn');
        loading.present();
        this.form.disable();
        this.regError = null;
        this.loginError = null;

        // Talk to our api and register them
        const regResp = await this.userService.registerExternalProvider(
            this.providerResult.provider,
            this.form.get('name').value,
            this.form.get('username').value
        );

        // Stop if response is an exception
        if (regResp instanceof CustomError) {
            loading.dismiss();
            this.regError = regResp;
            return;
        }

        // Login user
        const loginResp = await this.authService.loginWithExternalProvider(this.providerResult.provider, this.providerResult.user.token);

        // Hide loading
        loading.dismiss();
        this.form.enable();

        // Stop if response is an exception
        if (loginResp instanceof CustomError) {
            this.loginError = loginResp;
            return;
        }

        this.modalCtrl.dismiss();
    }

    // Sets the form data
    //
    private setForm() {
        this.form = this.fb.group({
            name: [this.user.name, userValidators('name')],
            email: [this.user.email, userValidators('email')],
            username: ['', userValidators('username')],
            terms: [false, [Validators.requiredTrue]],
        });
    }
}
