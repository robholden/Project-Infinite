export function stringFormat(input: string, ...values: string[]) {
    if (!input) return '';
    if ((values || []).length === 0) return input;

    return input.replace(/{(\d+)}/g, (match, number) => {
        return typeof values[number] != 'undefined' ? values[number] : match;
    });
}
