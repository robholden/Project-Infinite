export function maskEmail(email: string): string {
    return maskUntil(email, '@');
}

export function maskMobile(mobile: string): string {
    return maskText(mobile, { start: 0, end: 2 });
}

export function maskText(text: string, opts: { start: number; end: number }, skip?: string[]): string {
    if (!text) return text;

    skip = skip || [];

    let value: string = '';
    for (let i = 0; i < text.length; i++) {
        if (skip.includes(text[i]) || i < opts.start || i > opts.end) value += text[i];
        else value += '•';
    }

    return value;
}

export function maskUntil(text: string, char: string): string {
    if (!text) return text;

    const i = text.indexOf(char);
    if (i < 0) return text;

    return maskText(text, { start: 0, end: i - 1 });
}
