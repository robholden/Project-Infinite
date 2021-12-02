export function formatStringForQuery(value: any) {
    if (typeof value !== 'string') return value;
    return encodeURI((value || '').trim().replace(' ', '+'));
}

export function formatStringFromQuery(value: any) {
    if (typeof value !== 'string') return value;
    return decodeURI((value || '').replace('+', ' '));
}
