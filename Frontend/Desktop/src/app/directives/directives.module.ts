import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { TippyDirective } from './tippy.directive';

@NgModule({
    imports: [CommonModule],
    declarations: [TippyDirective],
    exports: [TippyDirective],
})
export class DirectivesModule {}
