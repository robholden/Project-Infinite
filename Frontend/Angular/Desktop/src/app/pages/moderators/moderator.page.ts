import { Component, OnInit } from '@angular/core';

import { AuthState } from '@shared/storage';

@Component({
    selector: 'pi-moderator',
    templateUrl: 'moderator.page.html',
    styleUrls: ['moderator.page.scss'],
})
export class ModeratorPage implements OnInit {
    constructor(public authState: AuthState) {}

    ngOnInit() {}
}
