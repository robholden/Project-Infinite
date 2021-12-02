import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { ComponentsModule } from '@app/components/components.module';

import { IonicModule } from '@ionic/angular';

import { EditDetailsPage } from './edit-details.page';
import { UpdateEmailModal } from './modals/update-email/update-email.modal';
import { UpdateNameModal } from './modals/update-name/update-name.modal';
import { UpdateUsernameModal } from './modals/update-username/update-username.modal';
import { UpdateUserFieldComponent } from './update-user-field/update-user-field.component';

const routes: Routes = [
    {
        path: '',
        component: EditDetailsPage,
    },
];

@NgModule({
    imports: [CommonModule, FormsModule, ReactiveFormsModule, IonicModule, RouterModule.forChild(routes), ComponentsModule],
    declarations: [EditDetailsPage, UpdateNameModal, UpdateEmailModal, UpdateUsernameModal, UpdateUserFieldComponent],
})
export class EditDetailsPageModule {}
