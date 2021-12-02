import { Directive, ElementRef, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';

import { AppState } from '@shared/storage/app.state';

import tippy, { Content, Placement, Props } from 'tippy.js';

@Directive({
    selector: '[tippy]',
})
export class TippyDirective implements OnInit {
    instance: any;
    night_mode: boolean;

    @Input('tippy') public tippy: Content | TemplateRef<any>;
    @Input('placement') public placement: Placement = 'auto';
    @Input('hideOnFocus') public hideOnFocus: boolean;

    constructor(private el: ElementRef, private appState: AppState, private viewContainerRef: ViewContainerRef) {}

    public ngOnInit() {
        this.appState.nightMode$.subscribe((night_mode) => this.set(night_mode));
        this.appState.language$.subscribe(() => this.set());
    }

    private set(night_mode: boolean = null) {
        let content: Content;
        if (this.tippy instanceof TemplateRef) {
            const view = this.viewContainerRef.createEmbeddedView(<TemplateRef<any>>this.tippy);
            content = view.rootNodes[0];
        } else content = this.tippy;

        if (night_mode === null) night_mode = this.night_mode;
        else this.night_mode = night_mode;

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
