import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';

import { promptForPassword } from '@app/functions/password-prompt.fn';
import { RefresherAwaitDo } from '@app/functions/refresher-awaiter.fn';

import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { AlertController, IonItemSliding } from '@ionic/angular';
import { Storage } from '@ionic/storage-angular';

import { dateAsNow, updateObject, waitThen } from '@shared/functions';
import { CustomError, PagedList, PageRequest, PasswordRequest, Session } from '@shared/models';
import { SocketService } from '@shared/services';
import { AuthService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

import { Haptics, ImpactStyle } from '@capacitor/haptics';

@Component({
    selector: 'pi-sessions',
    templateUrl: './sessions.page.html',
    styleUrls: ['./sessions.page.scss'],
    animations: [
        trigger('fade', [
            state('in', style({ opacity: 1 })),
            transition(':enter', [style({ opacity: 0 }), animate(250)]),
            transition(':leave', animate(250, style({ opacity: 0 }))),
        ]),
        trigger('swipeOut', [transition(':leave', animate(250, style({ height: 0 })))]),
    ],
})
export class SessionsPage implements OnInit {
    pager: PageRequest = new PageRequest();
    sessions: Session[];
    result: PagedList<Session>;
    deleted: { [key: number]: boolean } = {};
    getting: boolean;
    deleting: boolean;
    authToken: string;

    private _pw: PasswordRequest = null;

    constructor(
        public authState: AuthState,
        private authService: AuthService,
        private alertCtrl: AlertController,
        private sockets: SocketService,
        private fingerprint: FingerprintAIO,
        private storage: Storage
    ) {}

    ngOnInit() {
        this.get();

        this.sockets.on('SessionAdded', 'sessions_page', () => this.get());
        this.sockets.on('SessionRevoked', 'sessions_page', async (authToken) => {
            if (!authToken || authToken === (await this.authState.authToken())) return;
            this.get();
        });
    }

    private async get() {
        // Tell ui we're loading
        this.getting = true;

        this.authToken = await this.authState.authToken();
        const resp = await this.authService.getSessions(this.pager);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            this.getting = false;
            return;
        }

        // If page 1 replace set
        this.result = resp;
        this.setSessions();

        // Stop loading
        this.getting = false;
    }

    async refresh(event: any) {
        this.pager.page = 1;
        await RefresherAwaitDo(this.get(), () => event.target.complete());
    }

    headerFn(index: number, records: Session[]) {
        const record = records[index];
        const oldDate = index === 0 ? null : dateAsNow(records[index - 1].updated);
        const thisDate = dateAsNow(record.updated);

        return oldDate !== thisDate ? thisDate : null;
    }

    async loadMore(event: any) {
        this.pager.page += 1;
        await this.get();
        event.target.complete();
    }

    /**
     * Called when ion-item-sliding is being swiped. If ratio is halfway open it fully.
     */
    async swiping(index: number, event: any, slider: IonItemSliding) {
        const item = event.target;
        const ratio = Math.floor(((await item.getOpenAmount()) / item.clientWidth) * 100);
        if (ratio >= 50) {
            if (this.deleting) return;

            slider.disabled = true;
            this.tapticImpact();

            await waitThen(50, async () => {
                await this.triggerDelete(index, slider);
                slider.disabled = false;
            });
        }
    }

    async triggerDelete(index: number, slider: IonItemSliding) {
        this.deleting = true;

        const cancel = async () => {
            await slider.close();
            this.deleting = false;
        };

        if (this._pw) {
            const alert = await this.alertCtrl.create({
                subHeader: 'Are you sure you remove this session?',
                buttons: [
                    {
                        text: 'Cancel',
                        role: 'cancel',
                        cssClass: 'color-medium',
                        handler: async () => await cancel(),
                    },
                    {
                        text: 'Remove',
                        cssClass: 'color-danger',
                        handler: async () => await this.delete(index, this._pw),
                    },
                ],
            });
            await alert.present();
        } else {
            const passwordRequest = await promptForPassword(
                {
                    title: 'Remove Session',
                    touchId: { message: 'Scan your fingerprint to remove this session' },
                    password: { message: 'Enter your password to remove this session', submitText: 'Remove', submitColor: 'danger' },
                },
                this.alertCtrl,
                this.fingerprint,
                this.storage,
                this.authState
            );
            if (passwordRequest === null) await cancel();
            else await this.delete(index, passwordRequest);
        }
    }

    private async delete(index: number, passwordRequest: PasswordRequest) {
        // Instantly delete
        const item = document.getElementById('item-' + index);
        const boundry = item.getBoundingClientRect();
        item.style.transform = `translate3d(-${boundry.width}px, 0px, 0px)`;
        this.deleted[index] = true;

        // Now go to api
        const resp = await this.authService.deleteSession(this.sessions[index].authTokenId, passwordRequest);
        if (resp instanceof CustomError) {
            this.deleted[index] = false;
            item.style.transform = `translate3d(0px, 0px, 0px)`;
        } else {
            this._pw = passwordRequest;
        }

        this.deleting = false;
    }

    private tapticImpact() {
        Haptics.impact({
            style: ImpactStyle.Heavy,
        });
    }

    /**
     * Parses sessions and updates array without replacing it
     */
    private setSessions() {
        this.deleted = {};

        if (!this.sessions) this.sessions = [];
        if (!this.result || !this.result.rows || this.result.rows.length === 0) return;

        this.result.rows.forEach((session, i) => {
            const index = this.sessions.findIndex((s) => s.authTokenId === session.authTokenId);
            if (index < 0) {
                if (i < this.sessions.length - 1) this.sessions.splice(i, 0, session);
                else this.sessions.push(session);
            } else updateObject(session, this.sessions[index]);
        });

        if (this.pager.page === 1) {
            this.sessions.forEach((session, i) => {
                const index = this.result.rows.findIndex((s) => s.authTokenId === session.authTokenId);
                if (index < 0) this.sessions.splice(i, 1);
            });
        }
    }
}
