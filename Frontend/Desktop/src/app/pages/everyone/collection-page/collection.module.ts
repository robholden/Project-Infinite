import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { LoadingTitlePlaceholder } from '@app/functions/routing-titles.fn';
import { ModalsModule } from '@app/modals/modal.module';
import { SharedModule } from '@app/shared/shared.module';

import { CollectionPage } from './collection.page';

const routes: Routes = [
    {
        path: ':collectionId',
        component: CollectionPage,
        data: { title: LoadingTitlePlaceholder },
    },
];

@NgModule({
    declarations: [CollectionPage],
    imports: [SharedModule, FormsModule, ComponentsModule, ModalsModule, RouterModule.forChild(routes)],
})
export class CollectionPageModule {}
