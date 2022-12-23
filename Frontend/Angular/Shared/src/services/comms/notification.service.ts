import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { CustomError, Notification, PagedList, PageRequest } from '@shared/models';

import { HttpApiService } from '../http-api.service';

export interface INotificationService {
    get(pageRequest: PageRequest): Promise<PagedList<Notification> | CustomError>;
    latestTotal(): Promise<number>;
    unreadTotal(): Promise<number>;
    markAsRead(id: string): Promise<void | CustomError>;
    markAllAsRead(): Promise<void | CustomError>;
    latest: number;
}

@Injectable({
    providedIn: 'root',
})
export class NotificationService implements INotificationService {
    latest: number = null;

    constructor(private api: HttpApiService) {}

    /**
     * Gets list of notifications for the logged in user
     *
     * @param pageRequest Page options
     */
    async get(pageRequest: PageRequest): Promise<PagedList<Notification> | CustomError> {
        return await this.api.get('/comms/notification?' + obj2QueryString(pageRequest), {
            toastError: false,
        });
    }

    /**
     * Returns the number of new notifications for the logged in user
     */
    async latestTotal(): Promise<number> {
        if (this.latest !== null) return this.latest;

        const resp = await this.api.get<number>('/comms/notification/count/latest', { toastError: false });
        this.latest = resp instanceof CustomError ? 0 : resp;

        return this.latest;
    }

    /**
     * Returns the number of un-read notifications for the logged in user
     */
    async unreadTotal(): Promise<number> {
        const resp = await this.api.get<number>('/comms/notification/count/unread', { toastError: false });
        return resp instanceof CustomError ? 0 : resp || 0;
    }

    /**
     * Marks a notification as read
     *
     * @param id The notification id
     */
    async markAsRead(id: string): Promise<void | CustomError> {
        return await this.api.put('/comms/notification/' + id, null, { toastError: false });
    }

    /**
     * Marks all notifications as read
     */
    async markAllAsRead(): Promise<void | CustomError> {
        return await this.api.put('/comms/notification');
    }
}
