import { DatePipe } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

import { toUtc } from '@shared/functions';

@Pipe({
    name: 'utc',
    pure: false,
})
export class DateUtcPipe implements PipeTransform {
    constructor(private datePipe: DatePipe) {}

    transform(value: Date | string, format?: string): string {
        return this.datePipe.transform(toUtc(value), format);
    }
}
