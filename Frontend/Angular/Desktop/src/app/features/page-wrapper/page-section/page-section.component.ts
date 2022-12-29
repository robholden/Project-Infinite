import { Component, Input } from '@angular/core';

export interface PageSectionImg {
    src: string;
    type: 'bg' | 'img';
    alt?: string;
}

@Component({
    selector: 'pi-page-section',
    templateUrl: './page-section.component.html',
    styleUrls: ['page-section.component.scss'],
})
export class PageSectionComponent {
    @Input() align: 'left' | 'right' = 'left';
    @Input() img: PageSectionImg;

    constructor() {}
}
