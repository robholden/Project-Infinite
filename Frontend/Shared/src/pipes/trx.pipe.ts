import { Inject, Pipe, PipeTransform } from '@angular/core';

import { TranslateService } from '@ngx-translate/core';

import { trxTransform } from '@shared/functions';
import { INJ_TRANSLATION } from '@shared/injectors';

@Pipe({
    name: 'trx',
    pure: false,
})
export class TrxPipe implements PipeTransform {
    constructor(@Inject(INJ_TRANSLATION) private translate: TranslateService) {}

    transform(value: any, params?: Object, text?: string): any {
        return trxTransform(this.translate, value, params, text);
    }
}
