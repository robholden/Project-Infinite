import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/component.module';
import { SharedModule } from '@app/shared/shared.module';

import { LocationsPage } from './locations.page';

const routes: Routes = [
    {
        path: '',
        component: LocationsPage,
        data: { title: 'Locations' },
    },
];

@NgModule({
    imports: [SharedModule, FormsModule, ComponentsModule, RouterModule.forChild(routes)],
    declarations: [LocationsPage],
})
export class LocationsPageModule {}
