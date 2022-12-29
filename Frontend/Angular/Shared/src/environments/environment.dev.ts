import { Environment } from '@shared/interfaces';

export class DevEnvironment implements Environment {
    public appTitle = 'Project Infinite';
    public refresh = '/identity/auth/validate';
    public httpTimeout = 1000 * 60 * 2;
    public production = false;
    public recaptchaKey?: string;

    constructor(public gateway = '//localhost:7000') {}
}
