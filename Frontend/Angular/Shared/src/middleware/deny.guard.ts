import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateChild, Router, RouterStateSnapshot } from '@angular/router';

import { AuthState } from '@shared/storage';

@Injectable({
    providedIn: 'root',
})
export class DenyGuard implements CanActivateChild {
    constructor(private authState: AuthState, private router: Router) {}

    async canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        const token: string = await this.authState.retrieve('token');
        if (token) {
            this.router.navigate(['/']);
            return false;
        }

        return true;
    }
}
