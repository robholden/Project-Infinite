import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { CustomError, Notification, PagedList, PageRequest } from '@shared/models';

import { HttpApiService } from '../http-api.service';
import { INotificationService } from './';

@Injectable({
    providedIn: 'root',
})
export class NotificationModService implements INotificationService {
    latest: number = null;

    constructor(private api: HttpApiService) {}

    /**
     * Gets list of notifications
     *
     * @param pageRequest Page options
     */
    async get(pageRequest: PageRequest): Promise<PagedList<Notification> | CustomError> {
        return await this.api.get('/comms/notification/mod?' + obj2QueryString(pageRequest), {
            toastError: false,
        });
    }

    /**
     * Returns the number of active notifications
     */
    async latestTotal(): Promise<number> {
        if (this.latest !== null) return this.latest;

        const resp = await this.api.get<number>('/comms/notification/mod/count/latest', { toastError: false });
        this.latest = resp instanceof CustomError ? 0 : resp;

        return this.latest;
    }

    /**
     * Returns the number of un-read notifications for the logged in user
     */
    async unreadTotal(): Promise<number> {
        const resp = await this.api.get<number>('/comms/notification/mod/count/unread', { toastError: false });
        return resp instanceof CustomError ? 0 : resp || 0;
    }

    /**
     * Marks a notification as read
     *
     * @param id The notification id
     */
    async markAsRead(id: string): Promise<void | CustomError> {
        return await this.api.put('/comms/notification/mod/' + id, null, { toastError: false });
    }

    /**
     * Marks all notifications as read
     */
    async markAllAsRead(): Promise<void | CustomError> {
        return await this.api.put('/comms/notification/mod');
    }
}
