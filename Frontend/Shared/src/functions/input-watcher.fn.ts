import { ElementRef } from '@angular/core';

import { fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';

export function inputWatcher(el: ElementRef<any>, changeEvt: (value: string) => void) {
    if (!el) return;

    fromEvent(el.nativeElement, 'keyup')
        .pipe(
            map((event: any) => event.target.value),
            debounceTime(500),
            distinctUntilChanged()
        )
        .subscribe((value: string) => {
            changeEvt(value);
            el.nativeElement.focus();
        });
}
