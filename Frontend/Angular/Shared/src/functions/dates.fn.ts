import { format, getTime } from 'date-fns';

export function dateAsNow(date: Date): string {
    date = new Date(date);
    date.setHours(0);
    date.setMinutes(0);
    date.setSeconds(0, 0);

    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
    const yesterday = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 1).getTime();
    const given = date.getTime();

    if (given === today) return 'Today';
    else if (given === yesterday) return 'Yesterday';

    return format(date, 'd MMMM yyyy');
}

export function toUtc(date: Date | string) {
    if (typeof date === 'string') date = new Date(date);

    const utc = Date.UTC(
        date.getFullYear(),
        date.getMonth(),
        date.getDate(),
        date.getHours(),
        date.getMinutes(),
        date.getSeconds(),
        date.getMilliseconds()
    );
    return new Date(utc);
}

export function dateIsBigger(control: Date, comparer: Date) {
    if (!control && comparer) return true;
    else if (!comparer && control) return false;

    return control < comparer;
}

export function dateDiff(date1: Date, date2: Date, format: 'ms' | 's' | 'm' | 'h' | 'd') {
    const t1 = new Date(date1 || new Date()).getTime();
    const t2 = new Date(date2 || new Date()).getTime();
    const diffTime = Math.abs(t2 - t1);

    switch (format) {
        case 'ms':
            return diffTime;

        case 's':
            return Math.ceil(diffTime / 1000);

        case 'm':
            return Math.ceil(diffTime / (1000 * 60));

        case 'h':
            return Math.ceil(diffTime / (1000 * 60 * 60));

        case 'd':
            return Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        case 'd':
            return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    }
}
