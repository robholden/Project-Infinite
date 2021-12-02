import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

import { PipesModule } from '@shared/pipes/pipes.module';

import { AlertComponent } from './alert/template/alert.component';
import { ControllersService } from './controllers.service';
import { LoadingComponent } from './loading/template/loading.component';
import { ModalComponent } from './modal/modal.component';
import { ModalController } from './modal/modal.controller';
import { ToastComponent } from './toast/template/toast.component';

@NgModule({
    imports: [CommonModule, ReactiveFormsModule, PipesModule],
    declarations: [ModalComponent, ToastComponent, LoadingComponent, AlertComponent],
    entryComponents: [ModalComponent, ToastComponent, LoadingComponent, AlertComponent],
    exports: [],
    providers: [ControllersService, ModalController],
})
export class ControllersModule {}
