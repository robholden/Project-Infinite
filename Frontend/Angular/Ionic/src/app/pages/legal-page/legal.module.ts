import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ModalsModule } from '@app/modals/modals.module';

import { IonicModule } from '@ionic/angular';

import { LegalPage } from './legal.page';

const routes: Routes = [
    {
        path: '',
        component: LegalPage,
    },
];

@NgModule({
    imports: [CommonModule, FormsModule, IonicModule, RouterModule.forChild(routes), ModalsModule],
    declarations: [LegalPage],
    entryComponents: [],
})
export class LegalPageModule {}
