import { TranslateService } from '@ngx-translate/core';

import { Trx } from '@shared/models';

import { pascalToScores, stringFormat } from './';

export function trxTransform(translate: TranslateService, key: any, params?: Object, text?: string, startKey?: string): string {
    if (!startKey && typeof key === 'string') startKey = key;
    if (!key) return key;

    if (key instanceof Trx) {
        text = key.fallback;
        params = key.params;
        key = key.key;
    }

    if (typeof key !== 'string') {
        return key;
    }

    // Lookup text via key
    let trx: string = translate.instant(key, params);
    if (trx === key) {
        // Lookup by lowercase
        const value_lower = pascalToScores(key).toLowerCase();
        trx = translate.instant(value_lower, params);

        if (trx === value_lower || typeof trx === 'object') {
            // Look for shared value
            if (!startKey && !key.startsWith('shared')) {
                const parts = key.split('.');
                let sharedKey = parts.reduce((acc, k, i) => {
                    if (i === 0) acc = 'shared';
                    else acc += `.${k}`;

                    return acc;
                }, '');

                let sharedText = trxTransform(translate, sharedKey, params, text, startKey);
                if (!!sharedText) return sharedText;

                sharedKey = 'shared.' + parts[parts.length - 1];
                sharedText = trxTransform(translate, sharedKey, params, text, startKey);
                if (!!sharedText) return sharedText;
            }

            // Use text as key, else use fallback text
            if (text) trx = translate.instant(text, params);
            else if (startKey) trx = startKey;
        }
    }

    if (params instanceof Array) {
        trx = stringFormat(trx, ...params);
    }

    return trx;
}
