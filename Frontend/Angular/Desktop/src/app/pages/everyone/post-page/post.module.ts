import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { PostPage } from './post.page';
import { EditPostComponent } from './edit-post/edit-post.component';
import { MarkdownModule } from 'ngx-markdown';
import { LoadingTitlePlaceholder } from '@app/functions/routing-titles.fn';

const routes: Routes = [
    {
        path: ':id',
        component: PostPage,
        data: { title: LoadingTitlePlaceholder },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes), MarkdownModule.forRoot()],
    declarations: [PostPage, EditPostComponent],
})
export class PostPageModule {}
