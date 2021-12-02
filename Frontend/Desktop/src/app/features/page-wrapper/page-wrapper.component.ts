import { Component, HostBinding, Input } from '@angular/core';

import { AppColour } from '@shared/types';

@Component({
    selector: 'sc-page',
    templateUrl: './page-wrapper.component.html',
    styleUrls: ['page-wrapper.component.scss'],
})
export class PageWrapperComponent {
    @HostBinding('attr.background') @Input() background: AppColour = 'light';
    @HostBinding('class.small') @Input() small: boolean = false;

    constructor() {}
}
