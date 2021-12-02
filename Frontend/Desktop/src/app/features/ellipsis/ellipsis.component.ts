import { Component, Input, OnInit } from '@angular/core';

import { EllipsisPipe } from '@shared/pipes/ellipsis.pipe';

@Component({
    selector: 'sc-ellipsis',
    templateUrl: './ellipsis.component.html',
    styleUrls: ['./ellipsis.component.scss'],
    providers: [EllipsisPipe],
})
export class EllipsisComponent implements OnInit {
    @Input() value: string;
    @Input() charLimit: number;
    @Input() ending: string = '...';
    @Input() endChar?: string;

    output: string;

    constructor(private ellipsis: EllipsisPipe) {}

    ngOnInit(): void {
        this.output = this.ellipsis.transform(this.value, this.charLimit, this.ending, this.endChar);
    }
}
