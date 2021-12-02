import { Component, Injector, OnInit } from '@angular/core';

import { PagedComponent } from '@app/features/paged-component.helper';

import { PageRequest, PictureModeration, PictureModSearch } from '@shared/models';
import { SocketService } from '@shared/services';
import { PictureModService } from '@shared/services/content';

@Component({
    selector: 'sc-approvals',
    templateUrl: './approvals.page.html',
    styleUrls: ['./approvals.page.scss'],
})
export class ApprovalsPage extends PagedComponent<PictureModeration, PictureModSearch> implements OnInit {
    constructor(injector: Injector, service: PictureModService, private sockets: SocketService) {
        super(injector, service, {
            pager: new PageRequest(),
            def: {},
            filterParams: [],
        });
    }

    ngOnInit(): void {
        this.sockets.on('ModeratedPicture', 'picture_page', async (pictureId: string, outcome: boolean) => {
            if (!this.result?.rows?.length) return;

            const i = this.result.rows.findIndex((r) => r.pictureId === pictureId);
            if (i < 0) return;

            this.result.rows.splice(i, 1);
        });
    }
}
