import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, CanActivateChild, CanLoad, Router, RouterStateSnapshot } from '@angular/router';

import { CustomEvent } from '@shared/enums/events.enum';
import { changeUrl, hasClaim } from '@shared/functions';
import { EventService } from '@shared/services/event.service';
import { AuthState } from '@shared/storage/auth.state';

@Injectable({
    providedIn: 'root',
})
export class AuthGuard implements CanActivate, CanActivateChild, CanLoad {
    constructor(private authState: AuthState, private events: EventService, private router: Router) {}

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        return this.logic(route, state);
    }

    public canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        return this.logic(route, state);
    }

    public async canLoad(): Promise<boolean> {
        const token = await this.authState.retrieve('token');
        return !!token;
    }

    private async logic(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        // Get token from storage
        const token = await this.authState.retrieve('token');

        // Verify claims data?
        const claimsAreOk = !Object.keys(route.data['claims'] || {}).some((key) => !hasClaim(token, route.data['claims'][key]));

        const roles: string[] = route.data['roles'] || [];
        if (route.parent) roles.push(...(route.parent.data['roles'] || []));
        const rolesAreOk = !roles.length || roles.some((r: string) => this.authState.roles[r]);

        // Only run logic when we have a token
        if (token) {
            if (!claimsAreOk || !rolesAreOk) {
                this.router.navigate(['/', { queryParams: { err: 'permissions' } }]);
            }

            return true;
        }

        // Require login
        const good = await this.events.trigger<boolean>(CustomEvent.Login, { ref: state.url });
        if (!good) setTimeout(() => changeUrl(state.url), 0);

        return good;
    }
}
