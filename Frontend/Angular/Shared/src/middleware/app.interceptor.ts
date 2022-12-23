import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';

import { forkJoin, from, Observable } from 'rxjs';
import { mergeMap } from 'rxjs/operators';

import { INJ_DEVICE, INJ_PLATFORM } from '@shared/injectors';
import { Device, Platform } from '@shared/interfaces';
import { AuthState } from '@shared/storage';

@Injectable({
    providedIn: 'root',
})
export class AppInterceptor implements HttpInterceptor {
    constructor(private authState: AuthState, @Inject(INJ_DEVICE) private device: Device, @Inject(INJ_PLATFORM) private platform: Platform) {}

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const tokenObs = from(this.authState.retrieve('token'));
        const uuidObs = from(this.device.uuid());

        return forkJoin({ token: tokenObs, uuid: uuidObs }).pipe(
            mergeMap((results) => {
                // Add authorization header
                if (results.token) {
                    request = request.clone({ headers: request.headers.set('Authorization', 'Bearer ' + results.token) });
                }

                // Add identity header
                if (this.device.uuid) {
                    request = request.clone({ headers: request.headers.set('x-identity-key', results.uuid) });
                }

                // Add platform header
                if (this.platform.name) {
                    request = request.clone({ headers: request.headers.set('x-platform', this.platform.name) });
                }

                // Send back event
                return next.handle(request);
            })
        );
    }
}
