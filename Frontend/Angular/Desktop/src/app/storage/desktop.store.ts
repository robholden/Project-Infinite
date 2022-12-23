import { Injectable } from '@angular/core';

import { CustomStore } from '@shared/storage';

type DesktopSchema = {
    preventRefresh: boolean;
};

@Injectable({
    providedIn: 'root',
})
export class DesktopStore extends CustomStore<DesktopSchema> {
    constructor() {
        super();
    }
}
