import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { IonicModule } from '@ionic/angular';

import { PipesModule } from '@shared/pipes/pipes.module';

import { PreferencesPage } from './preferences.page';

const routes: Routes = [
    {
        path: '',
        component: PreferencesPage,
    },
];

@NgModule({
    imports: [CommonModule, FormsModule, ReactiveFormsModule, IonicModule, RouterModule.forChild(routes), PipesModule],
    declarations: [PreferencesPage],
})
export class PreferencesPageModule {}
