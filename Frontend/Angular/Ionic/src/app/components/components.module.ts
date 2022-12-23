import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

import { IonicModule } from '@ionic/angular';

import { PipesModule } from '@shared/pipes/pipes.module';

import { CustomFieldComponent } from './custom-field/custom-field.component';
import { ExternalLoginsComponent } from './external-logins/external-logins.component';

@NgModule({
    declarations: [CustomFieldComponent, ExternalLoginsComponent],
    imports: [CommonModule, IonicModule, ReactiveFormsModule, PipesModule],
    exports: [CustomFieldComponent, ExternalLoginsComponent],
})
export class ComponentsModule {}
