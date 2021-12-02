import { Component, Inject, OnInit } from '@angular/core';

import { AlertController, IonRouterOutlet } from '@ionic/angular';

import { CustomEvent } from '@shared/enums';
import { INJ_ENV } from '@shared/injectors';
import { Environment } from '@shared/interfaces';
import { EventService, SocketService } from '@shared/services';
import { AppState, AuthState } from '@shared/storage';

import { App } from '@capacitor/app';

@Component({
    selector: 'sc-account',
    templateUrl: 'account.page.html',
    styleUrls: ['account.page.scss'],
})
export class AccountPage implements OnInit {
    version: string;
    CustomEvent = CustomEvent;

    constructor(
        @Inject(INJ_ENV) public env: Environment,
        public authState: AuthState,
        public appState: AppState,
        public events: EventService,
        public routerOutlet: IonRouterOutlet,
        private alertCtrl: AlertController,
        private sockets: SocketService
    ) {
        App.getInfo()
            .then((info) => (this.version = info.version))
            .catch(() => (this.version = 'DEV'));
    }

    ngOnInit() {}

    async sendAdminMessage() {
        const alert = await this.alertCtrl.create({
            header: 'Send Message',
            subHeader: 'What would you like to send?',
            inputs: [
                {
                    name: 'message',
                    type: 'text',
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    cssClass: 'color-medium',
                },
                {
                    text: 'Send',
                    cssClass: 'color-primary',
                    handler: async (inputs) => {
                        if (!inputs.message) return;
                        await this.sockets.invoke('SendMessageToAdmins', inputs.message);
                    },
                },
            ],
        });

        await alert.present();
    }
}
