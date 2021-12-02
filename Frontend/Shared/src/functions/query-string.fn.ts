import { SMap } from '@shared/models';

import { formatStringForQuery, isNull, parseValue, QueryType } from './';

export function changeUrl(url: string) {
    window.history.replaceState(null, '', url);
}

export function obj2QueryString(...objects: object[]): string {
    objects = (objects || []).filter((o) => !!o);
    if (objects.length === 0) return '';

    const combined: {} = {};

    objects.forEach((o: object) => {
        Object.keys(o).forEach((key) => {
            let value: any = o[key];
            if (typeof value !== 'undefined' && value !== null) {
                if (value instanceof Date) value = value.toJSON();
                else if (value instanceof Array)
                    value = value
                        .map((v) => formatStringForQuery(v))
                        .filter((v) => !!v)
                        .join(',');
                else value = formatStringForQuery(value);

                combined[key] = value;
            }
        });
    });

    const params: string[] = Object.entries(combined).reduce((acc, curr) => {
        acc.push(`${curr[0]}=${curr[1]}`);
        return acc;
    }, []);

    return params.join('&');
}

export function writeQueryString(params: object, ignore?: string[], def?: object) {
    (ignore || []).forEach((key) => delete params[key]);
    Object.keys(def || {}).forEach((key) => {
        if (key in params && params[key] == def[key]) delete params[key];
    });

    let url = location.pathname;
    const qi = url.indexOf('?');
    if (qi >= 0) url = url.split(/[?#]/)[0];

    const newparams = obj2QueryString(params);
    changeUrl(url + (newparams ? '?' + newparams : ''));
}

export function buildQueryString<T>(params: SMap<string>, config: SMap<QueryType>, def?: T): T {
    if (!params) return def;

    const keys: string[] = Object.keys(config) || [];
    if (!keys.length) return def;

    return keys.reduce((acc: T, key: string) => {
        if (!(key in params)) {
            if (!def || !(key in def)) return acc;
            else {
                acc[key] = def[key];
                return acc;
            }
        }

        if (!(key in config)) return acc;

        const value = parseValue(params[key], config[key]);
        if (isNull(value)) return acc;

        acc[key] = value;
        return acc;
    }, (def || {}) as T);
}
