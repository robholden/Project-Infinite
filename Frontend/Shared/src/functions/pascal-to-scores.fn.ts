export function pascalToScores(value: string): string {
    return (value || '').replace(/(?:^|\.?)([A-Z])/g, (x, y) => '_' + y.toLowerCase()).replace(/^_/, '');
}
