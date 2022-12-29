import { Environment } from '@shared/interfaces';

export class DesktopEnvironment implements Environment {
    appTitle: string;
    gateway: string;
    refresh: string;
    httpTimeout: number;
    production: boolean;
    recaptchaKey?: string = '6Le8ynMUAAAAADOWORojoFlX_AHuHdYTQaTnLCqY';

    facebook = {
        appId: '1111781849247647',
    };
    apple = {
        clientId: 'com.rholden.project-infinite.web',
        redirectUrl: 'https://project-infinite.eu.ngrok.io/login/apple',
    };
    google = {
        clientId: '779342722703-b2jo58nlk3linqhi2akqp8iehuqs37op.apps.googleusercontent.com',
    };

    constructor(env: Environment) {
        Object.keys(env).forEach((key) => (this[key] = env[key]));
    }
}
