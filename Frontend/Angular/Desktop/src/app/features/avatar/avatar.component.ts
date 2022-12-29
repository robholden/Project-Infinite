import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'pi-avatar',
    templateUrl: './avatar.component.html',
    styleUrls: ['./avatar.component.scss'],
})
export class AvatarComponent implements OnInit {
    @Input() name: string;
    @Input() bg: string;
    @Input() fg: string;
    @Input() size: number = 28;

    constructor() {}

    ngOnInit(): void {}
}
