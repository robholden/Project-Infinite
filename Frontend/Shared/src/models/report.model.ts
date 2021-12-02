export interface Report<R> {
    reportId: string;
    username: string;
    action: ReportAction;
    date: Date;
    stats: ReportStats<R>[];
    reports: ReportInstance[];
}

export interface UserReport extends Report<ReportUserReason> {
    name: string;
    email: string;
}

export interface PictureReport extends Report<ReportPictureReason> {
    pictureId: string;
    pictureName: string;
    picturePath: string;
}

export interface ReportInstance {
    username: string;
    reason: number;
    date: Date;
}

export interface ReportStats<R> {
    total: number;
    reason: R;
}

export interface ReportAction {
    username: string;
    action: ReportedAction;
    notes: string;
    created: Date;
}

export enum ReportedAction {
    Ignore,
    Confirm,
}

export enum ReportUserReason {
    Offensive,
    Impersonation,
}

export enum ReportPictureReason {
    Offensive,
    Copyright,
    Explicit,
}

export interface ReportSearch {
    type?: 'User' | 'Picture';
    actioned?: boolean;
}

export enum ReportOrderBy {
    None,
    Username,
    Date,
}
