import { Directive, ElementRef, Input, OnInit } from '@angular/core';

import { continueWhen, wait } from '@shared/functions';

declare const rowGrid: any;

@Directive({
    selector: '[grid]',
})
export class GridDirective implements OnInit {
    @Input() grid: string;
    @Input() height: number = 300;

    private element: HTMLElement;

    constructor(private el: ElementRef) {}

    ngOnInit() {
        this.element = this.el.nativeElement;

        setTimeout(() => this.load(), 0);
    }

    private async load() {
        const items = document.querySelectorAll(this.grid);
        if (!items.length) return;

        for (let i = 0; i < items.length; i++) {
            items[i].classList.add('grid-item');
        }

        const images: NodeListOf<HTMLImageElement> = this.element.querySelectorAll('img');
        for (let i = 0; i < images.length; i++) {
            const image = images[i];
            const ratio = this.height / image.height;

            image.height = this.height;
            image.width *= ratio;
        }

        this.element.classList.add('grid-loaded');

        await wait(0);

        var options = { minMargin: 10, maxMargin: 10, itemSelector: this.grid, resize: true };
        rowGrid(this.element, options);

        let nodes = this.element.childNodes.length;

        continueWhen(() => {
            const old = nodes;
            nodes = this.element.childNodes.length;

            return nodes !== old;
        }, 25).then(() => this.load());
    }
}
