import { SimpleChanges } from '@angular/core';

export function itemsAreEqual(obj: any, obj2: any): boolean {
    if (!obj && !obj2) return true;
    if (!obj || !obj2) return false;

    return JSON.stringify(obj) === JSON.stringify(obj2);
}

export function valuesHasChanged(changes: SimpleChanges, fields: string[]) {
    const results = (fields || []).map((f) => valueHasChanged(changes, f));
    return results.some((r) => r);
}

export function valueHasChanged(changes: SimpleChanges, field: string) {
    if (!changes) return false;

    const change = changes[field];
    if (!change || change.firstChange) return false;

    return !itemsAreEqual(change.currentValue, change.previousValue);
}

export function propHasChanged<T>(changes: SimpleChanges, field: string, condition: (previous: T, current: T) => boolean) {
    if (!valueHasChanged(changes, field)) return false;

    const change = changes[field];
    return condition(change.previousValue as T, change.currentValue as T);
}
