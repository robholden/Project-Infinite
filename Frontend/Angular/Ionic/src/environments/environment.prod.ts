import { ProdEnvironment } from '@shared/environments/environment.prod';
import { Environment } from '@shared/interfaces/env.interface';

export const environment: Environment = new ProdEnvironment();
