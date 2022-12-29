import { Component, HostBinding, Input, OnInit } from '@angular/core';

@Component({
    selector: 'pi-skeleton-text',
    templateUrl: './skeleton-text.component.html',
    styleUrls: ['skeleton-text.component.scss'],
})
export class SkeletonTextComponent implements OnInit {
    @Input() @HostBinding('style.height') height: string;
    @Input() @HostBinding('style.width') width: string;
    @Input() @HostBinding('style.margin') margin: string;
    @Input() @HostBinding('style.border-radius') radius: string;
    @Input() @HostBinding('class.animated') animated: boolean;
    @Input() @HostBinding('class.light') light: boolean;

    constructor() {}

    ngOnInit() {}
}
