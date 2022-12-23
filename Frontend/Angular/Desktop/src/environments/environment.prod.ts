import { ProdEnvironment } from '@shared/environments/environment.prod';
import { DesktopEnvironment } from './desktop-environment';

export const environment = new DesktopEnvironment(new ProdEnvironment());
