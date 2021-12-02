import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { TagsPage } from './tags.page';

import { SortablejsModule } from 'ngx-sortablejs';

const routes: Routes = [
    {
        path: '',
        component: TagsPage,
        data: { title: 'Tags' },
    },
];

@NgModule({
    imports: [SharedModule, FormsModule, ComponentsModule, SortablejsModule, RouterModule.forChild(routes)],
    declarations: [TagsPage],
})
export class TagsPageModule {}
