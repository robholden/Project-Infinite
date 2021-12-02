import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { GridDirective } from './grid.directive';
import { TippyDirective } from './tippy.directive';

@NgModule({
    imports: [CommonModule],
    declarations: [GridDirective, TippyDirective],
    exports: [GridDirective, TippyDirective],
})
export class DirectivesModule {}
