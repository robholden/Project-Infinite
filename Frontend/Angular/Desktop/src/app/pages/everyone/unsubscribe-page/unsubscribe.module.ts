import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { UnsubscribePage } from './unsubscribe.page';

const routes: Routes = [
    {
        path: ':key',
        component: UnsubscribePage,
        data: { title: 'Unsubscribe' },
    },
];

@NgModule({
    imports: [SharedModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [UnsubscribePage],
})
export class UnsubscribePageModule {}
