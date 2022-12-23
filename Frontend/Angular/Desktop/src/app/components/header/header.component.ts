import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Validators } from '@angular/forms';

import { AlertController } from '@app/shared/controllers/alert';

import { CustomEvent } from '@shared/enums';
import { waitThen } from '@shared/functions';
import { SMap } from '@shared/models';
import { EventService, LoadingService, SocketService } from '@shared/services';
import { AuthState } from '@shared/storage';

import { NotificationsComponent } from '../notifications/notifications.component';

@Component({
    selector: 'sc-header',
    templateUrl: './header.component.html',
    styleUrls: ['header.component.scss'],
})
export class HeaderComponent implements OnInit, OnDestroy {
    unread: SMap<string> = {};
    alerted: SMap<boolean> = {};

    @ViewChild('userNotif') notificationsRef!: NotificationsComponent;
    @ViewChild('modNotif') modNotificationsRef?: NotificationsComponent;

    readonly CustomEvent = CustomEvent;

    constructor(
        private sockets: SocketService,
        private alertCtrl: AlertController,
        public authState: AuthState,
        public events: EventService,
        public loading: LoadingService
    ) {}

    ngOnInit() {}

    ngOnDestroy() {}

    viewNotifications(type: 'user' | 'mod') {
        const ref = type === 'mod' ? this.modNotificationsRef : this.notificationsRef;
        ref.fetch();

        this.unread[type] = '';
    }

    async sendAdminMessage() {
        // Show prompt
        const inputs = await this.alertCtrl.create({
            title: 'Send Message',
            message: 'What would you like to send?',
            inputs: [
                {
                    name: 'message',
                    type: 'text',
                    validators: [Validators.required],
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    className: 'mr-a',
                },
                {
                    text: 'Submit',
                    role: 'submit',
                    colour: 'primary',
                    className: 'primary',
                },
            ],
        });
        if (!inputs) return;

        await this.sockets.invoke('SendMessageToAdmins', inputs.message);
    }

    async newNotification(userLevel: string, total: number) {
        this.unread[userLevel] = this.numberTotal(total);
        this.alerted[userLevel] = false;

        if (total > 0) await waitThen(0, () => (this.alerted[userLevel] = true));
    }

    private numberTotal(num: number) {
        if (num <= 0) return '';
        else if (num > 99) return '99+';
        else return num.toString();
    }
}
