import { DevEnvironment } from '@shared/environments/environment.dev';

import { DesktopEnvironment } from './desktop-environment';

export const environment = new DesktopEnvironment(new DevEnvironment());
