export function isAsync(fn: Function) {
    return fn.constructor.name === 'AsyncFunction';
}
