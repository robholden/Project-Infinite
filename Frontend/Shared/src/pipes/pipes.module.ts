import { CommonModule, DatePipe } from '@angular/common';
import { NgModule } from '@angular/core';

import { TranslateModule } from '@ngx-translate/core';

import { DateUtcPipe } from './date-utc.pipe';
import { EllipsisPipe } from './ellipsis.pipe';
import { ImageUrlPipe } from './image-url.pipe';
import { RemoteContentPipe } from './remote-content.pipe';
import { TimeAgoPipe } from './timeago.pipe';
import { TrxPipe } from './trx.pipe';

@NgModule({
    imports: [CommonModule, TranslateModule],
    declarations: [TrxPipe, TimeAgoPipe, EllipsisPipe, RemoteContentPipe, ImageUrlPipe, DateUtcPipe],
    exports: [TrxPipe, TimeAgoPipe, EllipsisPipe, RemoteContentPipe, ImageUrlPipe, DateUtcPipe],
    providers: [DatePipe],
})
export class PipesModule {}
