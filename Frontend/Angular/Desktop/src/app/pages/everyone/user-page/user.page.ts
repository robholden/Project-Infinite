import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

import { CustomEvent } from '@shared/enums';
import { changeUrl } from '@shared/functions';
import { CustomError, User } from '@shared/models';
import { EventService } from '@shared/services';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

import { fade } from '@app/functions/animations.fn';
import { replaceLoadingTitle } from '@app/functions/routing-titles.fn';
import { ModalController } from '@app/shared/controllers/modal';

import { ReportUserModal } from './report-user/report-user.modal';

@Component({
    selector: 'pi-user',
    templateUrl: './user.page.html',
    styleUrls: ['./user.page.scss'],
    animations: [fade],
})
export class UserPage implements OnInit {
    loading: boolean = true;
    user: User;
    author: boolean;
    username: string;
    action: string;
    reported: boolean;

    readonly CustomEvent = CustomEvent;

    constructor(
        private activatedRoute: ActivatedRoute,
        private title: Title,
        private modalCtrl: ModalController,
        private userService: UserService,
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

    async report() {
        if (this.reported) return;

        const modal = this.modalCtrl.add('report-user', ReportUserModal, { user: this.user });
        this.reported = await modal.present();
    }

    private async fetchUser() {
        const loggedInUser = this.authState.snapshot('user');
        this.author = loggedInUser && loggedInUser.username.toLowerCase() === this.username;
        if (this.author) {
            this.user = loggedInUser;
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

        this.loading = false;
        this.setUserTitle();
    }

    private async setUserTitle() {
        setTimeout(() => replaceLoadingTitle(this.title, this.user ? this.user.username : ':('), 0);
    }
}
