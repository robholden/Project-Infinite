export function objectsEqual(obj1: any, obj2: any): boolean {
    return JSON.stringify(obj1) === JSON.stringify(obj2);
}

export function updateObject(obj: any, newObj: any) {
    Object.keys(obj).forEach((key) => (newObj[key] = obj[key]));
}
