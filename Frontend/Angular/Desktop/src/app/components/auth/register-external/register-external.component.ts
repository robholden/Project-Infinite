import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { CustomError, ErrorCode, ProviderResult, Trx, User, userValidators } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, UserService } from '@shared/services/identity';

import { LoadingController } from '@app/shared/controllers/loading';
import { ModalComponent, ModalController } from '@app/shared/controllers/modal';
import { ToastController } from '@app/shared/controllers/toast';

@Component({
    selector: 'pi-register-external',
    templateUrl: './register-external.component.html',
    styleUrls: ['./register-external.component.scss'],
})
export class RegisterExternalComponent extends ModalComponent<boolean> implements OnInit {
    @Input() providerResult: ProviderResult;
    @Input() user: User;

    form: FormGroup<{ name: FormControl<string>; email: FormControl<string>; username: FormControl<string>; terms: FormControl<boolean> }>;
    regError: Trx;
    loginError: Trx;

    constructor(
        public events: EventService,
        private userService: UserService,
        private authService: AuthService,
        private toastCtrl: ToastController,
        private modalCtrl: ModalController,
        private loadingCtrl: LoadingController
    ) {
        super();
    }

    ngOnInit() {
        setTimeout(() => document.getElementById('name').focus(), 0);
        this.form = new FormGroup({
            name: new FormControl(this.user.name, userValidators('name')),
            email: new FormControl(this.user.email, userValidators('email')),
            username: new FormControl('', userValidators('username')),
            terms: new FormControl<boolean>(false, [Validators.requiredTrue]),
        });
    }

    async register() {
        // Tell ui we're loading
        const loading = this.loadingCtrl.addBtn('ext-register-btn');
        loading.present();
        this.form.disable();
        this.regError = null;
        this.loginError = null;

        // Talk to our api and register them
        const regResp = await this.userService.registerExternalProvider(this.providerResult.provider, this.form.value.name, this.form.value.username);

        // Stop if response is an exception
        if (regResp instanceof CustomError) {
            loading.dismiss();

            if (regResp.code === ErrorCode.HttpUnauthorized) {
                this.dismiss();
                this.toastCtrl.add('The session has expired, please try again', 'danger').present(5000);
            } else this.regError = regResp;

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
}
