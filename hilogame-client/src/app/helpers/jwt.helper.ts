export function decodeJwt<T = any>(token: string): T | null {
    try {
        const payload = token.split('.')[1];
        return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    } catch {
        return null;
    }
}

export function getJwtExp(token: string): number | null {
    const p = decodeJwt<{ exp: number }>(token);
    return p?.exp ?? null; // exp en secondes epoch
}

export function isJwtExpired(token: string, skewSec = 0): boolean {
    const exp = getJwtExp(token);
    if (!exp) return true;
    const now = Math.floor(Date.now() / 1000);
    return now >= (exp - skewSec);
}
