export function uniqueArray<T>(array: T[], matchCondition?: (cond1: T, cond2: T) => boolean): T[] {
    if (!matchCondition) matchCondition = (cond1: T, cond2: T) => cond1 === cond2;

    return array.reduce((acc, curr) => {
        const exists = acc.some((a) => matchCondition(a, curr));
        if (!exists) acc.push(curr);
        return acc;
    }, [] as T[]);
}
