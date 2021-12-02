import { ExternalProvider, TwoFactorType } from '@shared/enums';

import { User } from './user.model';

export interface LoginResponse {
    token: string;
    refreshToken: string;
    authToken: string;
    user: User;
    twoFactorRequired: TwoFactorType;
    provider: ExternalProvider;
}
