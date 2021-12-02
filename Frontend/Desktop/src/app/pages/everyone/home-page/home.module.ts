import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { HomePage } from './home.page';

const routes: Routes = [
    {
        path: '',
        component: HomePage,
        data: { title: 'Home' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [HomePage],
    entryComponents: [],
})
export class HomePageModule {}
