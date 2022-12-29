import { Component, EventEmitter, HostBinding, Input, OnInit, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'pi-toggle-switch',
    templateUrl: 'toggle-switch.component.html',
    styleUrls: ['toggle-switch.component.scss'],
})
export class ToggleSwitchComponent implements OnInit {
    @Input() checked: boolean = false;
    @Output() checkedChange = new EventEmitter<boolean>();

    @Input() parent: FormGroup;
    @Input() name: string;
    @HostBinding('class.sm') @Input() small: boolean;

    constructor() {}

    ngOnInit() {}
}
