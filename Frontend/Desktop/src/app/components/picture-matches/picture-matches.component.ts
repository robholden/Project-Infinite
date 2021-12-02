import { Component, Input, OnInit } from '@angular/core';

import { CustomError, Picture } from '@shared/models';
import { PictureService } from '@shared/services/content';

@Component({
    selector: 'sc-picture-matches',
    templateUrl: './picture-matches.component.html',
    styleUrls: ['./picture-matches.component.scss'],
})
export class PictureMatchesComponent implements OnInit {
    @Input() pictureId: string;
    @Input() autoload: boolean = false;

    loading: boolean = true;
    matches: Picture[];

    constructor(private service: PictureService) {}

    ngOnInit(): void {
        if (this.autoload) this.fetch();
    }

    async fetch() {
        this.loading = true;
        const results = await this.service.matches(this.pictureId);
        this.loading = false;

        if (results instanceof CustomError) {
            return;
        }

        this.matches = results;
    }
}
