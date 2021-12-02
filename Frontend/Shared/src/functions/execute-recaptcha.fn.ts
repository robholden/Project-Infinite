declare const grecaptcha: any;

export function executeReCaptcha(reCaptchaKey: string, action: string): Promise<string> {
    return new Promise((res) => {
        try {
            grecaptcha
                .execute(reCaptchaKey, { action })
                .then((token: string) => res(token))
                .catch(() => res(null));
        } catch (err) {
            res(null);
        }
    });
}

export function recaptchaLoaded(): Promise<boolean> {
    return new Promise((res) => {
        try {
            grecaptcha.ready(() => res(true));
        } catch (err) {
            res(false);
        }
    });
}
