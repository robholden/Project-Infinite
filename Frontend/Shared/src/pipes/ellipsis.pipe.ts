import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'ellipsis',
    pure: false,
})
export class EllipsisPipe implements PipeTransform {
    transform(value: string, char_limit: number, ending: string = '...', end_char?: string): string {
        value = value || '';
        if (value.length <= char_limit) return value;

        if (!end_char) {
            return value.substr(0, char_limit - 1) + ending;
        }

        const values = value.split(end_char);
        const end = end_char + values.pop();
        const new_value = values.join(end_char).substr(0, char_limit - 1);

        return `span title="${value}">${new_value}${ending}${end}</span>`;
    }
}
