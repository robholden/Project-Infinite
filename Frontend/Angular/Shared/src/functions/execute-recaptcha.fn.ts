import { continueWhen } from './wait.fn';

declare const grecaptcha: any;

let loaded = false;

export const executeReCaptcha = async (reCaptchaKey: string, action: string): Promise<string> => {
    // Ensure grecaptcha is ready before executing
    await continueWhen(() => loaded, 50, 10 * 1000);

    // If we haven't loaded return null
    if (!loaded) {
        console.error('Failed to load ReCapatcha!');
        return null;
    }

    // Execute recaptcha
    return await new Promise((res) => {
        try {
            grecaptcha
                .execute(reCaptchaKey, { action })
                .then((token: string) => res(token))
                .catch(() => res(null));
        } catch (err) {
            console.error('Recapctca Failed!', err);
            res(null);
        }
    });
};

export const recaptchaLoaded = async (): Promise<boolean> => {
    loaded = await new Promise((res) => {
        try {
            grecaptcha.ready(() => res(true));
        } catch (err) {
            res(false);
        }
    });
    return loaded;
};
