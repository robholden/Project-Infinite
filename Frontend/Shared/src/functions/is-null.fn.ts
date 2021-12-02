export function isNull(value: any) {
    return typeof value === 'undefined' || value === null;
}

export function nullCheck<T>(value: T, defaultValue: T) {
    return isNull(value) ? defaultValue : value;
}
