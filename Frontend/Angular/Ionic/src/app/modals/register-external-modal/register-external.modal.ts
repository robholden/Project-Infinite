import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { ModalWithParams } from '@app/functions/setup-events.fn';

import { LoadingController, ModalController, ToastController } from '@ionic/angular';

import { CustomEvent } from '@shared/enums';
import { CustomError, User, userValidators } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService, UserService } from '@shared/services/identity';

export type ExternalProvider = 'facebook' | 'google' | 'apple';

export class ProviderResult {
    constructor(public provider: ExternalProvider, public token: string) {}
}

@Component({
    selector: 'sc-register-external',
    templateUrl: './register-external.modal.html',
    styleUrls: ['./register-external.modal.scss'],
})
export class RegisterExternalModal implements OnInit, ModalWithParams {
    @Input() providerResult: ProviderResult;
    @Input() user: User;

    ref: string;
    form: FormGroup;
    CustomEvent = CustomEvent;
    presentingElement?: HTMLIonModalElement | HTMLIonRouterOutletElement;

    get name() {
        return this.form.get('name');
    }

    get username() {
        return this.form.get('username');
    }

    get terms() {
        return this.form.get('terms');
    }

    constructor(
        public events: EventService,
        private router: Router,
        private fb: FormBuilder,
        private userService: UserService,
        private authService: AuthService,
        private modalCtrl: ModalController,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController
    ) {}

    ngOnInit() {
        this.setForm();
    }

    async dismiss() {
        await this.modalCtrl.dismiss('register');
    }

    async register() {
        // Tell ui we're loading
        const loading = await this.loadingCtrl.create({
            message: 'Registering',
            spinner: 'crescent',
        });
        await loading.present();

        // Talk to our api and register them
        const regResp = await this.userService.registerExternalProvider(this.providerResult.provider, this.name.value, this.username.value, {
            toastError: true,
        });

        // Stop if response is an exception
        if (regResp instanceof CustomError) {
            await loading.dismiss();
            return;
        }

        // Login user
        const loginResp = await this.authService.loginWithExternalProvider(this.providerResult.provider, this.providerResult.token);

        // Stop if response is an exception
        if (loginResp instanceof CustomError) {
            await loading.dismiss();
            return;
        }

        // Registration successful
        const toast = await this.toastCtrl.create({
            message: `Thanks for registering ${regResp.name}!`,
            color: 'primary',
            duration: 2500,
        });

        await this.router.navigate([this.ref ? this.ref : this.router.url]);
        await this.dismiss();
        await loading.dismiss();
        await toast.present();
    }

    private setForm() {
        this.form = this.fb.group({
            name: [this.user.name, userValidators('name')],
            email: [this.user.email, userValidators('email')],
            username: ['', userValidators('username')],
            terms: [false, [Validators.requiredTrue]],
        });
    }
}
