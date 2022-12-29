import { Component, Input } from '@angular/core';

export interface PageLink {
    name: string;
    url: string;
    visible?: boolean;
}

@Component({
    selector: 'pi-page-header',
    templateUrl: './page-header.component.html',
    styleUrls: ['page-header.component.scss'],
})
export class PageHeaderComponent {
    @Input() pageLinks: PageLink[] = [];

    constructor() {}

    linkClicked(target: any) {
        target?.blur();
    }
}
