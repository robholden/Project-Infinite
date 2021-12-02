import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { IonicModule } from '@ionic/angular';

import { SessionsPage } from './sessions.page';

const routes: Routes = [
    {
        path: '',
        component: SessionsPage,
    },
];

@NgModule({
    imports: [CommonModule, FormsModule, IonicModule, RouterModule.forChild(routes)],
    declarations: [SessionsPage],
})
export class SessionsPageModule {}
