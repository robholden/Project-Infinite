export function hasParent(el: HTMLElement, parentSelector: HTMLElement | ((parentEl: HTMLElement) => boolean)) {
    if (el === parentSelector) return true;
    else if (typeof parentSelector === 'function' && parentSelector(el)) return true;

    let p = el.parentNode;
    while (p !== null) {
        let o = p as any;

        if (o === parentSelector) return true;
        else if (o === document) return false;
        else if (typeof parentSelector === 'function' && parentSelector(o)) return true;

        p = o.parentNode;
    }

    return false;
}
