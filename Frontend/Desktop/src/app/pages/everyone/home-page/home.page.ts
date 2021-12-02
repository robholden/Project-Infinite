import { Component, OnInit } from '@angular/core';

import { PagedList, Picture } from '@shared/models';

@Component({
    selector: 'sc-home',
    templateUrl: 'home.page.html',
    styleUrls: ['home.page.scss'],
})
export class HomePage implements OnInit {
    results: PagedList<Picture>;

    constructor() {}

    ngOnInit() {}
}
