import { Pipe, PipeTransform } from '@angular/core';

import { toUtc } from '@shared/functions';

@Pipe({
    name: 'timeago',
    pure: false,
})
export class TimeAgoPipe implements PipeTransform {
    transform(value: Date, isUtc: boolean = true): string {
        const date = isUtc ? toUtc(value) : new Date(value);
        if (!date) return '';

        const seconds = new Date().getTime() / 1000 - date.getTime() / 1000;
        const minutes = Math.ceil(seconds / 60);
        const hours = Math.ceil(minutes / 60);
        const days = Math.ceil(hours / 24);

        const weeks = Math.ceil(days / 7);

        if (seconds < 60) return 'Just now';
        if (minutes < 60) return `${minutes} minute${this.addS(minutes)} ago`;
        if (hours < 24) return `${hours} hour${this.addS(hours)} ago`;
        if (days < 7) return `${days} day${this.addS(days)} ago`;

        return `${weeks} week${this.addS(weeks)} ago`;
    }

    private addS(num: number): string {
        return num === 1 ? '' : 's';
    }
}
