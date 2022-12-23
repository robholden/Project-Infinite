export interface Environment {
    appTitle: string;
    gateway: string;
    refresh: string;
    httpTimeout: number;
    production: boolean;
    recaptchaKey?: string;
}
