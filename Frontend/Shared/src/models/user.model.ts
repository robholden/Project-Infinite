import { Validators } from '@angular/forms';

import { TwoFactorType } from '@shared/enums/two-factor-type.enum';

export type UserField = 'name' | 'username' | 'email' | 'mobile';

export type Provider = 'google' | 'facebook' | 'apple';

export class SocialUser {
    constructor(public token: string, public name?: string) {}
}

export class ProviderResult {
    constructor(public provider: Provider, public user: SocialUser) {}
}

export interface User {
    userId: string;
    name: string;
    username: string;
    email: string;
    emailConfirmed: boolean;
    mobile: string;
    twoFactorType: TwoFactorType;
    twoFactorEnabled: boolean;
    created: Date;
    lastActive: Date;
    status: number;
    acceptedTerms: boolean;
}

export function userValidators(field: UserField) {
    switch (field) {
        case 'name':
            return [Validators.required, Validators.maxLength(100)];

        case 'email':
            return [Validators.required, Validators.email, Validators.maxLength(255)];

        case 'username':
            return [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern('[a-zA-Z0-9_]*')];

        default:
            return [];
    }
}
