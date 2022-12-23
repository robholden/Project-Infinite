import { Observable, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

export function wait(ms: number) {
    return new Promise<void>((res) => setTimeout(() => res(), ms));
}

export function waitFor<T>(obs: Observable<T>, continueWhen: (result: T) => boolean, valueOnError?: T): Promise<boolean> {
    return new Promise<boolean>((res) => {
        let sub = obs
            .pipe(
                catchError(() => of(valueOnError)),
                finalize(() => {
                    sub.unsubscribe();
                    sub = null;
                })
            )
            .subscribe(
                (result) => {
                    if (continueWhen(result)) {
                        res(true);
                    }
                },
                () => {
                    if (continueWhen(valueOnError)) {
                        res(false);
                        sub.unsubscribe();
                        sub = null;
                    }
                }
            );
    });
}

export function continueWhen(condition: () => Promise<boolean> | boolean, pollInMs: number = 500, stopAtMs?: number): Promise<boolean> {
    let total_ms = 0;
    return new Promise<boolean>(async (res) => {
        while (!stopAtMs || total_ms < stopAtMs) {
            if (await condition()) {
                res(true);
                break;
            }

            await wait(pollInMs);
            total_ms += pollInMs;
        }
    });
}

export function waitThen(ms: number, then: Function) {
    return new Promise<void>((res) =>
        setTimeout(() => {
            then();
            res();
        }, ms)
    );
}
