import { Directive, ElementRef, Input, OnDestroy, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AppState } from '@shared/storage/app.state';

import tippy, { Content, Placement, Props } from 'tippy.js';

@Directive({
    selector: '[tippy]',
})
export class TippyDirective implements OnInit, OnDestroy {
    private destroy$ = new Subject();

    instance: any;
    nightMode: boolean;

    @Input('tippy') public tippy: Content | TemplateRef<any>;
    @Input('placement') public placement: Placement = 'auto';
    @Input('hideOnFocus') public hideOnFocus: boolean;

    constructor(private el: ElementRef, private appState: AppState, private viewContainerRef: ViewContainerRef) {}

    ngOnInit() {
        this.appState
            .observe('nightMode')
            .pipe(takeUntil(this.destroy$))
            .subscribe((nightMode) => this.set(nightMode));

        this.appState.language$.pipe(takeUntil(this.destroy$)).subscribe(() => this.set());
    }

    ngOnDestroy(): void {
        this.destroy$.next();
    }

    private set(night_mode: boolean = this.nightMode) {
        console.log(this.tippy);

        let content: Content;
        if (this.tippy instanceof TemplateRef) {
            const view = this.viewContainerRef.createEmbeddedView(<TemplateRef<any>>this.tippy);
            content = view.rootNodes[0];
        } else content = this.tippy;

        if (night_mode === null) night_mode = this.nightMode;
        else this.nightMode = night_mode;

        const opts: Partial<Props> = {
            duration: [150, 150],
            inertia: true,
            animation: 'shift-away',
            content,
            placement: this.placement,
            theme: night_mode ? 'light' : '',
        };

        if (this.instance) this.instance.setProps(opts);
        else this.instance = tippy(this.el.nativeElement, opts);

        if (this.hideOnFocus) {
            if (document.activeElement === this.el.nativeElement) this.instance.disable();
            this.el.nativeElement.onfocus = () => this.instance.disable();
            this.el.nativeElement.onblur = () => this.instance.enable();
        }
    }
}
