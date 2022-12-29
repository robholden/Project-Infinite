import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { CustomError, Notification, NotificationType, PagedList, PageRequest } from '@shared/models';
import { SocketService } from '@shared/services';
import { INotificationService, NotificationModService, NotificationService } from '@shared/services/comms';
import { AuthState } from '@shared/storage';

import { LoadingController } from '@app/shared/controllers/loading';

@Component({
    selector: 'pi-notifications',
    templateUrl: './notifications.component.html',
    styleUrls: ['./notifications.component.scss'],
})
export class NotificationsComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject();
    private pager: PageRequest;

    @Input() mod: boolean;
    @Output() latest = new EventEmitter<number>();

    service: INotificationService;
    loading: boolean = true;
    results: PagedList<Notification>;
    unread: number = 0;

    readonly NotificationType = NotificationType;

    constructor(
        private authState: AuthState,
        private sockets: SocketService,
        private userService: NotificationService,
        private modService: NotificationModService,
        private loadingCtrl: LoadingController
    ) {}

    ngOnInit(): void {
        this.service = this.mod ? this.modService : this.userService;

        this.authState
            .observe('authToken')
            .pipe(takeUntil(this.destroy$))
            .subscribe((token) => {
                if (!token) return;

                this.socketConnector(true);
                this.emitLatest();
            });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.socketConnector(false);
    }

    async fetch() {
        this.pager = new PageRequest();

        this.loading = true;
        const [results, unread] = await Promise.all([this.service.get(this.pager), this.service.unreadTotal()]);
        this.loading = false;

        if (results instanceof CustomError) {
            return;
        }

        this.unread = unread;
        this.results = results;

        this.service.latest = 0;
        this.latest.emit(0);
    }

    async received(notification: Notification) {
        if (!this.results)
            this.results = {
                hasNextPage: false,
                hasPreviousPage: false,
                pageNumber: 1,
                rows: [],
                timestamp: '',
                totalPages: 1,
                totalRows: 0,
            };

        const i = this.results.rows.findIndex((r) => r.notificationId === notification.notificationId);
        if (i >= 0) {
            this.results.rows[i] = notification;
            if (!notification.isGlobal) this.unread++;
        } else {
            this.results.rows.unshift(notification);
            this.results.totalRows++;
        }

        this.service.latest++;
        this.latest.emit(this.service.latest);
    }

    async loadMore() {
        this.pager.page++;

        const loading = this.loadingCtrl.addBtn('load-more-btn');
        loading.present();

        const results = await this.service.get(this.pager);
        loading.dismiss();

        if (results instanceof CustomError) {
            return;
        }

        this.results.hasNextPage = results.hasNextPage;
        this.results.hasPreviousPage = results.hasPreviousPage;
        this.results.rows.push(...results.rows);
        this.results.totalPages = results.totalPages;
        this.results.totalRows = results.totalRows;
        this.results.timestamp = results.timestamp;
    }

    async markAsRead(notification: Notification) {
        if (notification.read) return;
        notification.read = true;

        const resp = await this.service.markAsRead(notification.notificationId);
        if (resp instanceof CustomError) notification.read = false;
        else this.unread--;
    }

    async markAllAsRead() {
        const loading = this.loadingCtrl.addBtn('unread-btn');
        loading.present();

        const resp = await this.service.markAllAsRead();
        loading.dismiss();

        if (resp instanceof CustomError) {
            return;
        }

        this.results.rows.forEach((n) => (n.read = true));
        this.unread = 0;
    }

    private async emitLatest() {
        this.latest.emit(await this.service.latestTotal());
    }

    private async socketConnector(on: boolean) {
        const group = `${this.mod ? 'Moderator' : ''}NewNotification`;
        const key = 'notification';

        if (on) this.sockets.on(group, key, (notification: Notification) => this.received(notification));
        else this.sockets.off(group, key);
    }
}
