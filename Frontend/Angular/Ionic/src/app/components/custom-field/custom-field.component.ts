import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, Input, OnInit } from '@angular/core';
import { AbstractControl } from '@angular/forms';

import { SMap } from '@shared/models';

@Component({
    selector: 'sc-custom-field',
    templateUrl: './custom-field.component.html',
    styleUrls: ['./custom-field.component.scss'],
    animations: [
        trigger('fade', [
            state('in', style({ opacity: 1, transform: 'translateY(0)' })),
            transition(':enter', [style({ opacity: 0, transform: 'translateY(5px)' }), animate(100)]),
            transition(':leave', animate(100, style({ opacity: 0, transform: 'translateY(5px)' }))),
        ]),
    ],
})
export class CustomFieldComponent implements OnInit {
    @Input() label: string;
    @Input() field: AbstractControl;
    @Input() errors: string[] = [];
    @Input() trxParams: SMap<any> = {};

    constructor() {}

    ngOnInit() {}

    errorKey(err: string) {
        return typeof this.trxParams[err] === 'string' ? this.trxParams[err] : err;
    }
}
