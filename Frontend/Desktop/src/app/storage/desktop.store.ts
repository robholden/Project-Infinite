import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class DesktopStore {
    private prevent_refreshes: number = 0;

    get shouldPreventRefresh(): boolean {
        return this.prevent_refreshes > 0;
    }

    preventRefresh(yes: boolean) {
        this.prevent_refreshes += yes ? 1 : -1;
    }
}
