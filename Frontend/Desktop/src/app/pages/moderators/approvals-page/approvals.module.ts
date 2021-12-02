import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { ApprovalsPage } from './approvals.page';

const routes: Routes = [
    {
        path: '',
        component: ApprovalsPage,
        data: { title: 'Approvals' },
    },
];

@NgModule({
    imports: [SharedModule, FormsModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [ApprovalsPage],
    entryComponents: [],
    exports: [],
})
export class ApprovalsPageModule {}
