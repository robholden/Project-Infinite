import { trigger } from '@angular/animations';
import { Component, OnInit, ViewChild } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

import { CustomEvent } from '@shared/enums';
import { changeUrl } from '@shared/functions';
import { CustomError, User, UserStats } from '@shared/models';
import { EventService } from '@shared/services';
import { StatsService } from '@shared/services/content';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

import { fade } from '@app/functions/animations.fn';
import { replaceLoadingTitle } from '@app/functions/routing-titles.fn';
import { ChangeSettingsModal } from '@app/modals/change-settings/change-settings.modal';
import { ModalController } from '@app/shared/controllers/modal';

import { ReportUserModal } from './report-user/report-user.modal';
import { ShowCollectionsComponent } from './show-collections/show-collections.component';

@Component({
    selector: 'sc-user',
    templateUrl: './user.page.html',
    styleUrls: ['./user.page.scss'],
    animations: [fade],
})
export class UserPage implements OnInit {
    loading: boolean = true;
    user: User;
    stats: UserStats;
    author: boolean;
    username: string;
    action: string;
    reported: boolean;
    CustomEvent = CustomEvent;

    @ViewChild(ShowCollectionsComponent) collectionList!: ShowCollectionsComponent;

    constructor(
        private activatedRoute: ActivatedRoute,
        private title: Title,
        private modalCtrl: ModalController,
        private userService: UserService,
        private statsService: StatsService,
        public authState: AuthState,
        public events: EventService
    ) {}

    ngOnInit(): void {
        this.activatedRoute.params.subscribe((params) => {
            this.action = this.activatedRoute.snapshot.data['action'];
            const username: string = (params['username'] || '').toLowerCase();
            if (this.username !== username) {
                this.user = null;
                this.username = username;
                this.fetchUser();
            }
        });
    }

    changeAction(action: string, evt: any) {
        evt.preventDefault();
        if (action === this.action) return;

        this.action = action;
        changeUrl(`/user/${this.username}/${action}`);
    }

    async openNewCollection() {
        const collection = await this.events.trigger(CustomEvent.ModifyCollection);
        if (collection) this.collectionList.search();
    }

    async report() {
        if (this.reported) return;

        const modal = this.modalCtrl.add('report-user', ReportUserModal, { user: this.user });
        this.reported = await modal.present();
    }

    async changeSettings() {
        const modal = this.modalCtrl.add('change-user-settings', ChangeSettingsModal, { userId: this.user.userId });
        this.reported = await modal.present();
    }

    private async fetchUser() {
        this.author = this.authState.user && this.authState.user.username.toLowerCase() === this.username;
        if (this.author) {
            this.user = this.authState.user;
        }

        // Lookup user from api
        else {
            const userResp = await this.userService.get(this.username);

            // Stop if response is an exception
            if (userResp instanceof CustomError) {
                this.loading = false;
                this.setUserTitle();
                return;
            }

            this.user = userResp;
        }

        // Fetch stats for user
        this.stats = await this.statsService.get(this.username);
        if (this.author) this.authState.userStats = this.stats;

        this.loading = false;
        this.setUserTitle();
    }

    private async setUserTitle() {
        setTimeout(() => replaceLoadingTitle(this.title, this.user ? this.user.username : ':('), 0);
    }
}
