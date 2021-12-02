import { Component, EventEmitter, Input, Output } from '@angular/core';

import { AppColour } from '@shared/types';

@Component({
    selector: 'sc-modal-wrapper',
    templateUrl: 'modal-wrapper.component.html',
    styleUrls: ['modal-wrapper.component.scss'],
})
export class ModalWrapperComponent {
    @Input() background: AppColour = null;
    @Input() showClose: boolean = true;
    @Output() closed = new EventEmitter<boolean>();

    constructor() {}
}
