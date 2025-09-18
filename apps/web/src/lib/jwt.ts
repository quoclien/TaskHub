/**
 * JWT utility functions for token decoding and user information extraction
 */

interface JWTPayload {
  sub: string; // Subject (user ID)
  jti: string; // JWT ID
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string;
  exp: number; // Expiration time
  iss: string; // Issuer
  aud: string; // Audience
}

/**
 * Decode a JWT token and extract the payload
 * @param token - The JWT token to decode
 * @returns The decoded payload or null if invalid
 */
export const decodeJWT = (token: string): JWTPayload | null => {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) {
      return null;
    }

    const payload = parts[1];
    const decodedPayload = atob(payload);
    return JSON.parse(decodedPayload) as JWTPayload;
  } catch (error) {
    console.error('Failed to decode JWT token:', error);
    return null;
  }
};

/**
 * Extract user ID from JWT token
 * @param token - The JWT token
 * @returns The user ID or null if not found
 */
export const getUserIdFromToken = (token: string): string | null => {
  const payload = decodeJWT(token);
  return payload?.sub || null;
};

/**
 * Extract user role from JWT token
 * @param token - The JWT token
 * @returns The user role or null if not found
 */
export const getUserRoleFromToken = (token: string): string | null => {
  const payload = decodeJWT(token);
  return payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
};

/**
 * Check if JWT token is expired
 * @param token - The JWT token
 * @returns True if token is expired, false otherwise
 */
export const isTokenExpired = (token: string): boolean => {
  const payload = decodeJWT(token);
  if (!payload?.exp) {
    return true;
  }

  const currentTime = Math.floor(Date.now() / 1000);
  return payload.exp < currentTime;
};
