import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { LoadingTitlePlaceholder } from '@app/functions/routing-titles.fn';
import { ModalsModule } from '@app/modals/modal.module';
import { ApprovalsPageModule } from '@app/pages/moderators/approvals-page/approvals.module';
import { SharedModule } from '@app/shared/shared.module';

import { PicturePage } from './picture.page';

import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import { SortablejsModule } from 'ngx-sortablejs';

const routes: Routes = [
    {
        path: ':pictureId',
        component: PicturePage,
        data: { title: LoadingTitlePlaceholder },
    },
];

@NgModule({
    imports: [SharedModule, FormsModule, ComponentsModule, ModalsModule, RouterModule.forChild(routes), LeafletModule, ApprovalsPageModule],
    declarations: [PicturePage],
})
export class PicturePageModule {}
