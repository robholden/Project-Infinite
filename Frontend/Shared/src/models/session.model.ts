export interface Session {
    authTokenId: string;
    active: boolean;
    created: Date;
    expired: boolean;
    ipAddress: string;
    platform: string;
    twoFactorPassed: boolean;
    updated?: Date;
}
