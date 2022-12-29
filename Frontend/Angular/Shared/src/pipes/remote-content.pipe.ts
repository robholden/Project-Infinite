import { HttpClient } from '@angular/common/http';
import { Pipe, PipeTransform } from '@angular/core';

import { Observable, of, Subject } from 'rxjs';
import { catchError, delay, tap } from 'rxjs/operators';

import { SMap } from '@shared/models';

class HtmlCache {
    static cache: SMap<string> = {};
    static obs: SMap<Observable<string>> = {};
}

@Pipe({
    name: 'remoteContent',
})
export class RemoteContentPipe implements PipeTransform {
    constructor(private http: HttpClient) {}

    transform(url: string): Observable<string> {
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
