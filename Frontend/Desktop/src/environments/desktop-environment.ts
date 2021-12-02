import { Environment } from '@shared/interfaces';

export class DesktopEnvironment implements Environment {
    appTitle: string;
    gateway: string;
    refresh: string;
    httpTimeout: number;
    production: boolean;
    recaptchaKey?: string = '6Le8ynMUAAAAADOWORojoFlX_AHuHdYTQaTnLCqY';

    locationIQ = {
        token: 'pk.5fc6b053493bde59c3c62e1cfe451cb5',
    };
    facebook = {
        appId: '1111781849247647',
    };
    apple = {
        clientId: 'com.snowcapture.web',
        redirectUrl: 'https://snowcapture.eu.ngrok.io/login/apple',
    };
    google = {
        clientId: '862560448504-tuqume8imv1ueme6jkm91me52t138jm7.apps.googleusercontent.com',
    };

    constructor(env: Environment) {
        Object.keys(env).forEach((key) => (this[key] = env[key]));
    }
}
