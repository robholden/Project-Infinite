export interface Notification {
    notificationId: string;
    contentKey: string;
    contentMessage: string;
    contentImage: string;
    users: number;
    date: Date;
    viewed: boolean;
    read: boolean;
    type: NotificationType;
    isGlobal: boolean;
}

export enum NotificationType {
    None = 0,
    NewLike,
    PictureUnapproved,
    PictureApproved,
    NewPictureApproval,
}
