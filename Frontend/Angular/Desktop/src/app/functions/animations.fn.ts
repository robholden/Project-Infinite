import { animate, style, transition, trigger } from '@angular/animations';

export const fade = trigger('fade', [
    transition(':enter', [style({ opacity: 0 }), animate(500)]),
    transition(':leave', animate(0, style({ opacity: 0 }))),
]);
