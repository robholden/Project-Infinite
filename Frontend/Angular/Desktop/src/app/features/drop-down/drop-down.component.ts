import { Component, EventEmitter, HostBinding, Input, OnInit, Output } from '@angular/core';

import { CustomEvent } from '@shared/enums';
import { hasParent, wait } from '@shared/functions';
import { EventService } from '@shared/services';

class DropdownState {
    private static _index: number = 0;

    static get index(): number {
        this._index++;
        return this._index;
    }
}

@Component({
    selector: 'pi-drop-down',
    templateUrl: './drop-down.component.html',
})
export class DropDownComponent implements OnInit {
    @HostBinding('id') private _id = `dropdown_${DropdownState.index}`;

    @Input() position: 'topleft' | 'topright' | 'bottomleft' | 'bottomright' = 'bottomright';
    @Input() width: number = 0;
    @Input() x: number = 0;
    @Input() y: number = 0;
    @Input() small: boolean;
    @Input() maxHeight: number = null;

    @Output() opened = new EventEmitter();
    @Output() closed = new EventEmitter();

    constructor(private events: EventService) {}

    ngOnInit() {
        this.init();
    }

    private async init() {
        await wait(0);

        const button = document.getElementById(this._id).parentElement;
        const that = this;

        button.addEventListener('click', function () {
            if (this.classList.contains('opened') || this.classList.contains('opening')) return;

            this.classList.add('opening');
            that.events.trigger(CustomEvent.BodyClick, {
                id: that._id,
                fn: (evt: MouseEvent) => {
                    const target = evt.target as HTMLElement;
                    const self = hasParent(target, button);
                    const linkItem = hasParent(target, (el: HTMLElement) => el.hasAttribute('href') || el.hasAttribute('data-dropdown-link'));
                    if (self && !linkItem) return;

                    this.classList.remove('opening');
                    this.classList.remove('opened');

                    that.events.trigger(CustomEvent.BodyClick, { id: that._id, fn: null });

                    that.closed.emit();
                },
            });

            setTimeout(() => {
                that.opened.emit();
                this.classList.add('opened');
            }, 0);
        });
    }
}
