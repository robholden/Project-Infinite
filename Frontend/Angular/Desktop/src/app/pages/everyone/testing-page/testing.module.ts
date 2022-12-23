import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { TestPage } from './testing.page';

const routes: Routes = [
    {
        path: '',
        component: TestPage,
        data: { title: 'Test' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [TestPage]
})
export class TestingPageModule {}
