import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { PostPage } from './post.page';
import { EditPostComponent } from './edit-post/edit-post.component';

const routes: Routes = [
    {
        path: ':id',
        component: PostPage,
        data: { title: 'Post' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [PostPage, EditPostComponent],
})
export class PostPageModule {}
