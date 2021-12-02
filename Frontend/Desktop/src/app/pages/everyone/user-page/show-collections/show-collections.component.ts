import { Component, Injector, OnInit } from '@angular/core';

import { PagedComponent } from '@app/features/paged-component.helper';
import { fade } from '@app/functions/animations.fn';

import { enumValues, parseEnum } from '@shared/functions';
import { Collection, CollectionOrderBy, CollectionSearch, PageRequest } from '@shared/models';
import { CollectionService } from '@shared/services/content';

@Component({
    selector: 'sc-show-collections',
    templateUrl: './show-collections.component.html',
    styleUrls: ['./show-collections.component.scss'],
    animations: [fade],
})
export class ShowCollectionsComponent extends PagedComponent<Collection, CollectionSearch> implements OnInit {
    enum_values = enumValues(CollectionOrderBy, CollectionOrderBy.None);

    constructor(injector: Injector, service: CollectionService) {
        super(injector, service, {
            pager: new PageRequest(),
            def: {},
            filterParams: ['username'],
            queryConfig: { orderBy: (value: string) => parseEnum(value, CollectionOrderBy) },
        });
    }

    ngOnInit(): void {}
}
