import { Inject, Injectable } from '@angular/core';

import { continueWhen, loadScripts, recaptchaLoaded } from '@shared/functions';
import { INJ_ENV } from '@shared/injectors';
import { Environment } from '@shared/interfaces';

@Injectable({
    providedIn: 'root',
})
export class ReCaptchaService {
    constructor(@Inject(INJ_ENV) private env: Environment) {}

    /**
     * Load recaptcha script
     */
    async load(): Promise<void> {
        if (!this.env.recaptchaKey) return;

        await loadScripts({ src: `https://www.google.com/recaptcha/api.js?render=${this.env.recaptchaKey}` });
        await continueWhen(() => recaptchaLoaded(), 50, 5000);
    }
}
