import { Component, Inject } from '@angular/core';

import { INJ_ENV } from '@shared/injectors';
import { Environment } from '@shared/interfaces';
import { SiteLoaderService } from '@shared/services';

@Component({
    selector: 'pi-home',
    templateUrl: 'home.page.html',
    styleUrls: ['home.page.scss'],
})
export class HomePage {
    constructor(@Inject(INJ_ENV) public env: Environment, public siteLoader: SiteLoaderService) {}
}
