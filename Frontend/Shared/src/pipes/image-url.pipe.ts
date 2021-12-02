import { Inject, Pipe, PipeTransform } from '@angular/core';

import { INJ_ENV } from '@shared/injectors';
import { Environment } from '@shared/interfaces';

@Pipe({
    name: 'imgUrl',
    pure: false,
})
export class ImageUrlPipe implements PipeTransform {
    constructor(@Inject(INJ_ENV) private env: Environment) {}

    transform(value: string, thumbnail: boolean = true): string {
        if (!value) return '';

        const thumbQuery = thumbnail ? '/thumbnail' : '';
        return `${this.env.gateway}/api/content/images${thumbQuery}?name=${value}`;
    }
}
