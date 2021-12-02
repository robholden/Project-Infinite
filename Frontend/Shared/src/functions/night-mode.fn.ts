export function preferNightMode(): boolean {
    if (!('matchMedia' in window)) return false;

    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)');
    return prefersDark && prefersDark.matches;
}
