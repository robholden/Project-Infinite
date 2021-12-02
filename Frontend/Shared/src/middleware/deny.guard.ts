import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateChild, Router, RouterStateSnapshot } from '@angular/router';

import { StorageService } from '@shared/services';
import { AuthStore } from '@shared/storage';

@Injectable({
    providedIn: 'root',
})
export class DenyGuard implements CanActivateChild {
    constructor(private authStore: StorageService<AuthStore>, private router: Router) {}

    async canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        const token: string = await this.authStore.getFromStorage('token');
        if (token) {
            this.router.navigate(['/']);
            return false;
        }

        return true;
    }
}
