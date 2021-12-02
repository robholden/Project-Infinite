import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { FeaturesModule } from '@app/features/features.module';

import { INJ_DEVICE, INJ_PLATFORM, INJ_STORAGE, INJ_TOAST } from '@shared/injectors';

import { ControllersModule } from './controllers/controllers.module';
import { DeviceContract } from './implementations/device.contract';
import { PlatformContract } from './implementations/platform.contract';
import { StorageContract } from './implementations/storage.contract';
import { ToastContract } from './implementations/toast.contract';

import { CookieService } from 'ngx-cookie-service';

@NgModule({
    imports: [],
    providers: [
        CookieService,
        { provide: INJ_STORAGE, useClass: StorageContract },
        { provide: INJ_DEVICE, useClass: DeviceContract },
        { provide: INJ_PLATFORM, useClass: PlatformContract },
        { provide: INJ_TOAST, useClass: ToastContract },
    ],
    exports: [CommonModule, RouterModule, ReactiveFormsModule, ControllersModule, FeaturesModule],
})
export class SharedModule {}
