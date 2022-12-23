import { isNull } from './is-null.fn';
import { pascalToScores } from './pascal-to-scores.fn';
import { formatStringFromQuery } from './url-string.fn';

export function parseDate(date: string, def?: Date): Date {
    if (!date) return def;

    try {
        const d = new Date(date);
        return d;
    } catch (err) {
        return def;
    }
}

export function parseNum(num: string, def?: number): number {
    if (!num) return def;

    try {
        const n = parseInt(num, 10);
        return isNaN(n) ? def : n;
    } catch (err) {
        return def;
    }
}

export function parseBool(bool: string, def?: boolean): boolean {
    if (!bool) return def;

    if (bool.toLowerCase() === 'false') return false;
    else if (bool.toLowerCase() === 'true') return true;

    const num = parseNum(bool, null);
    if (num === 0) return false;
    else if (num === 1) return true;

    return def;
}

export function parseEnum(value: string, enumParam: any): string {
    value = (value || '').toLowerCase();

    for (let entry in enumParam) {
        if (!isNaN(parseInt(entry))) continue;
        if (entry.toLowerCase() === value) return value;
        if (pascalToScores(entry) === value) return value;
    }

    return null;
}

export function parseEnumValue(value: number, enumParam: any): number {
    if (isNull(value)) return null;

    for (let entry in enumParam) {
        const enumValue = parseInt(entry);
        if (isNaN(enumValue)) continue;
        if (enumValue === value) return value;
    }

    return null;
}

export type QueryType = 'array' | 'number' | 'bool' | 'date' | 'string' | ((value: string) => any);

export function parseValue(value: string, type: QueryType): any {
    switch (type) {
        case 'date':
            return parseDate(value);

        case 'number':
            return parseNum(value);

        case 'bool':
            return parseBool(value);

        case 'array':
            return (value || '')
                .split(',')
                .map((value) => formatStringFromQuery(value))
                .filter((v) => !!v);

        case 'string':
            return formatStringFromQuery((value || '').toLowerCase());

        default:
            return type(value);
    }
}
