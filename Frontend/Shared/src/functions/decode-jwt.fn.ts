import jwt_decode from 'jwt-decode';

export function decodeJWT(token: string): any {
    return jwt_decode(token);
}

export function getClaim(jwt: string, key: string): any {
    if (key === 'role') key = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

    try {
        const payload = decodeJWT(jwt);
        return payload[key];
    } catch (ex) {}

    return null;
}

export function hasClaim(jwt: string, key: string) {
    return getClaim(jwt, key) !== null;
}
