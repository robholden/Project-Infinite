import { Component, OnInit } from '@angular/core';

import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { IonRouterOutlet, ModalController } from '@ionic/angular';
import { Storage } from '@ionic/storage';

import { AuthService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

import { ChangePasswordModal } from './modals/change-password/change-password.modal';
import { RemoveAccountModal } from './modals/remove-account/remove-account.modal';

@Component({
    selector: 'pi-security',
    templateUrl: './security.page.html',
    styleUrls: ['./security.page.scss'],
})
export class SecurityPage implements OnInit {
    exc: any;
    touchIdPending: boolean = true;
    touchIdAllowed: boolean;
    touchIdEnabled: boolean;

    constructor(
        public authState: AuthState,
        private modalCtrl: ModalController,
        private routerOutlet: IonRouterOutlet,
        private authService: AuthService,
        private fingerprint: FingerprintAIO,
        private ionStorage: Storage
    ) {}

    async ngOnInit() {
        try {
            await this.fingerprint.isAvailable();

            this.touchIdAllowed = true;
            this.touchIdEnabled = !!(await this.ionStorage.get('touch-id-name'));
        } catch (err) {
            this.exc = err;
            this.touchIdAllowed = false;
        }

        this.touchIdPending = false;
    }

    async changePassword() {
        const modal = await this.modalCtrl.create({
            component: ChangePasswordModal,
            swipeToClose: true,
            presentingElement: this.routerOutlet.nativeEl,
        });
        await modal.present();
    }

    async deleteAccount() {
        const modal = await this.modalCtrl.create({
            component: RemoveAccountModal,
            swipeToClose: true,
            presentingElement: this.routerOutlet.nativeEl,
        });
        await modal.present();
    }

    async toggleTouchId() {
        this.touchIdPending = true;

        // As we're toggling, when not enabled... enable
        const enabling = !this.touchIdEnabled;
        const ok = await this.authService.enableDisableTouchId(enabling);
        if (!ok) {
            this.touchIdPending = false;
            return;
        }

        try {
            if (enabling) {
                await this.ionStorage.set('touch-id-name', this.authState.user.name);
                await this.fingerprint.registerBiometricSecret({
                    description: 'Use your fingerprint to login',
                    secret: await this.authState.authToken(),
                    disableBackup: false,
                });
            } else {
                await this.ionStorage.remove('touch-id-name');
            }
        } catch (err) {
            this.touchIdPending = false;
            return;
        }

        this.touchIdEnabled = enabling;
        this.touchIdPending = false;
    }
}
