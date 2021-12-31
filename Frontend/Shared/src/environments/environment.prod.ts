import { Environment } from '@shared/interfaces';

export class ProdEnvironment implements Environment {
    public appTitle = 'Snow Capture';
    public gateway = 'https://api.snowcapture.com';
    public refresh = '/identity/auth/validate';
    public httpTimeout = 1000 * 60 * 2;
    public production = true;
    public recaptchaKey?: string;
}
