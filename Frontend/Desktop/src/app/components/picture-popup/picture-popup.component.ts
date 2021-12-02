import { Component, Input, OnInit } from '@angular/core';

import { Picture } from '@shared/models';

@Component({
    selector: 'sc-picture-popup',
    templateUrl: './picture-popup.component.html',
    styleUrls: ['./picture-popup.component.scss'],
})
export class PicturePopupComponent implements OnInit {
    @Input() picture: Picture;

    constructor() {}

    ngOnInit(): void {}
}
