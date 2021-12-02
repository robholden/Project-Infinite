import { Component, OnDestroy, OnInit } from '@angular/core';
import { Validators } from '@angular/forms';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ToastController } from '@app/shared/controllers/toast';

import { dateAsNow, updateObject } from '@shared/functions';
import { CustomError, PagedList, PageRequest, Session } from '@shared/models';
import { SocketService } from '@shared/services';
import { AuthService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'sc-setting-sessions',
    templateUrl: './setting-sessions.page.html',
    styleUrls: ['./setting-sessions.page.scss'],
})
export class SettingSessionsPage implements OnInit, OnDestroy {
    pager: PageRequest = new PageRequest();
    sessions: Session[];
    result: PagedList<Session>;
    getting: boolean;
    authToken: string;

    private _pw: string;

    constructor(
        public authState: AuthState,
        private authService: AuthService,
        private alertCtrl: AlertController,
        private toastCtrl: ToastController,
        private loadingCtrl: LoadingController,
        private sockets: SocketService
    ) {}

    ngOnInit() {
        this.get();

        this.sockets.on('SessionAdded', 'sessions_page', () => this.get());
        this.sockets.on('SessionRevoked', 'sessions_page', async (authToken) => {
            if (!authToken || authToken === (await this.authState.authToken())) return;
            this.get();
        });
    }

    ngOnDestroy() {
        this.sockets.off('SessionAdded', 'sessions_page').off('SessionRevoked', 'sessions_page');
    }

    private async get() {
        this.authToken = await this.authState.authToken();

        if (this.getting || !this.authState.loggedIn) return;
        this.getting = true;

        const resp = await this.authService.getSessions(this.pager);

        this.getting = false;

        if (resp instanceof CustomError) {
            return;
        }

        this.result = resp;
        this.setSessions();
    }

    headerFn(index: number, records: Session[]) {
        const record = records[index];
        const oldDate = index === 0 ? null : dateAsNow(records[index - 1].updated);
        const thisDate = dateAsNow(record.updated);

        return oldDate !== thisDate ? thisDate : null;
    }

    async loadMore() {
        this.pager.page += 1;
        await this.get();
    }

    async triggerDelete(index: number = null) {
        // Kill all?
        const kill_all = index === null;

        // Have they already entered their password?
        let password: string = null;
        const confirm_title = kill_all ? 'End All Sessions' : 'End Session';
        const confirm_text = kill_all ? 'This action will end all sessions, including your own' : 'Are you sure you want end this session?';
        const confirm_btn = kill_all ? 'End All' : 'End Session';
        if (this._pw || !!this.authState.provider) {
            const confirmed = await this.alertCtrl.confirm({
                title: confirm_title,
                message: `<p class="colour-danger">${confirm_text}</p>`,
                confirmBtn: {
                    text: confirm_btn,
                    colour: 'danger',
                },
                focusFirst: true,
            });
            if (!confirmed) return;
            else password = this._pw;
        } else {
            const result = await this.alertCtrl.create({
                title: confirm_title,
                message: `<p class="colour-danger">${confirm_text}</p>`,
                inputs: [
                    {
                        name: 'password',
                        type: 'password',
                        autocomplete: 'new-password',
                        placeholder: 'Enter your password',
                        validators: [Validators.required, Validators.minLength(6)],
                    },
                ],
                buttons: [
                    {
                        text: 'Cancel',
                        role: 'cancel',
                        className: 'mr-a',
                    },
                    {
                        text: confirm_btn,
                        role: 'submit',
                        colour: 'danger',
                        className: 'ml-a',
                    },
                ],
            });

            if (!result || !result.password) return;
            else password = result.password;
        }

        // Tell ui we're loading
        const loading = this.loadingCtrl.add(kill_all ? 'Ending All Sessions' : 'Ending Session');
        loading.present();

        // Now go to api
        const resp = kill_all
            ? await this.authService.deleteAll({ password })
            : await this.authService.deleteSession(this.sessions[index].authTokenId, { password });

        // Hide loading
        await loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        // Save pw and remove session
        this.sessions.splice(index, 1);
        this._pw = password;

        // Show toast success
        if (kill_all) {
            const toast = this.toastCtrl.add('All sessions have been ended', 'success');
            toast.present(5000);
        }
    }

    private setSessions() {
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
