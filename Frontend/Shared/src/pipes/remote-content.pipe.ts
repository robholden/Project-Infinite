import { HttpClient } from '@angular/common/http';
import { Inject, Pipe, PipeTransform } from '@angular/core';

import { Observable, of, Subject } from 'rxjs';
import { catchError, delay, tap } from 'rxjs/operators';

import { INJ_ENV } from '@shared/injectors';
import { Environment } from '@shared/interfaces';
import { SMap } from '@shared/models';

class HtmlCache {
    static cache: SMap<string> = {};
    static obs: SMap<Observable<string>> = {};
}

@Pipe({
    name: 'remoteContent',
})
export class RemoteContentPipe implements PipeTransform {
    constructor(@Inject(INJ_ENV) private env: Environment, private http: HttpClient) {}

    transform(name: string, type: string): Observable<string> {
        const url = `${this.env.gateway}/api/content/template/${type}/${name}`;

        if (HtmlCache.cache[url]) return of(HtmlCache.cache[url]);
        else if (HtmlCache.obs[url]) return HtmlCache.obs[url];

        const subject = new Subject<string>();
        HtmlCache.obs[url] = subject.asObservable();

        return this.http.get(url, { responseType: 'text' }).pipe(
            delay(0),
            catchError(() => of('')),
            tap((html) => {
                HtmlCache.cache[url] = html;
                subject.next(html);
            })
        );
    }
}
