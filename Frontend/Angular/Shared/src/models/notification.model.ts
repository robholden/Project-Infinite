export interface Notification {
    notificationId: string;
    type: NotificationType;
    identifier: string;
    contentRoute: string;
    contentImageUrl: string;
    users: number;
    date: Date;
    viewed: boolean;
    read: boolean;
    isGlobal: boolean;
}

export enum NotificationType {
    None = 0,
    NewLogin,
}
