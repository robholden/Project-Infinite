export function isPasswordValid(pw: string): boolean {
    return (pw || '').length >= 6;
}
