import { InjectionToken } from '@angular/core';

import { TranslateService } from '@ngx-translate/core';

import { Device, Environment, Platform, Toast } from './interfaces';
import { Storage } from './interfaces/storage.interface';

export const INJ_TRANSLATION = new InjectionToken<TranslateService>('INJ.TRANSLATION');
export const INJ_DEVICE = new InjectionToken<Device>('INJ.DEVICE');
export const INJ_ENV = new InjectionToken<Environment>('INJ.ENV');
export const INJ_PLATFORM = new InjectionToken<Platform>('INJ.PLATFORM');
export const INJ_TOAST = new InjectionToken<Toast>('INJ.TOAST');
export const INJ_STORAGE = new InjectionToken<Storage>('INJ.STORAGE');
