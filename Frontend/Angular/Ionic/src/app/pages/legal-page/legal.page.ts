import { Component, OnInit } from '@angular/core';

import { IonRouterOutlet } from '@ionic/angular';

import { CustomEvent } from '@shared/enums';
import { EventService } from '@shared/services';

@Component({
    selector: 'pi-legal',
    templateUrl: './legal.page.html',
    styleUrls: ['./legal.page.scss'],
})
export class LegalPage implements OnInit {
    CustomEvent = CustomEvent;

    constructor(public events: EventService, public routerOutlet: IonRouterOutlet) {}

    ngOnInit() {}
}
