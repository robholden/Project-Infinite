import { Inject, Pipe, PipeTransform } from '@angular/core';

import { TranslateService } from '@ngx-translate/core';

import { trxTransform } from '@shared/functions';
import { INJ_TRANSLATION } from '@shared/injectors';

@Pipe({
    name: 'trx',
    pure: false,
})
export class TrxPipe implements PipeTransform {
    private cached_value: any;
    private cached_params?: Object;
    private cached_text?: string;

    private cached_result: string;

    constructor(@Inject(INJ_TRANSLATION) private translate: TranslateService) {}

    transform(value: any, params?: Object, text?: string): any {
        if (this.cached_result && this.cached_value === value && this.cached_params === params && this.cached_text === text) {
            return this.cached_result;
        }

        this.cached_value = value;
        this.cached_params = params;
        this.cached_text = text;

        return (this.cached_result = trxTransform(this.translate, value, params, text));
    }
}
