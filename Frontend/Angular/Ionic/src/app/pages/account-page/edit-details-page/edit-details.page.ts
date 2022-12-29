import { Component, OnInit } from '@angular/core';

import { IonRouterOutlet, ModalController } from '@ionic/angular';

import { maskEmail } from '@shared/functions';
import { User, UserField } from '@shared/models';
import { AuthState } from '@shared/storage';

import { UpdateEmailModal } from './modals/update-email/update-email.modal';
import { UpdateNameModal } from './modals/update-name/update-name.modal';
import { UpdateUsernameModal } from './modals/update-username/update-username.modal';

@Component({
    selector: 'pi-edit-details',
    templateUrl: './edit-details.page.html',
    styleUrls: ['./edit-details.page.scss'],
})
export class EditDetailsPage implements OnInit {
    resending: boolean;
    sent: boolean;
    maskEmail: boolean = true;

    get email(): string {
        if (!this.user) return '';

        return this.maskEmail ? maskEmail(this.user.email) : this.user.email;
    }

    get user(): User {
        return this.authState.user;
    }

    constructor(private authState: AuthState, private routerOutlet: IonRouterOutlet, private modalCtrl: ModalController) {}

    ngOnInit() {}

    async editField(field: UserField) {
        let component;
        switch (field) {
            case 'email':
                component = UpdateEmailModal;
                break;

            case 'username':
                component = UpdateUsernameModal;
                break;

            default:
                component = UpdateNameModal;
                break;
        }

        const modal = await this.modalCtrl.create({
            component: component,
            swipeToClose: true,
            presentingElement: this.routerOutlet.nativeEl,
        });
        await modal.present();
    }
}
