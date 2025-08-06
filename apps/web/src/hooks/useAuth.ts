'use client';

import { useSession, signIn, signOut } from 'next-auth/react';
import { useCallback } from 'react';

export function useAuth() {
  const { data: session, status } = useSession();
  
  const login = useCallback((provider?: string) => {
    return signIn(provider || 'google', { callbackUrl: '/boards' });
  }, []);

  const logout = useCallback(() => {
    return signOut({ callbackUrl: '/' });
  }, []);

  return {
    user: session?.user,
    session,
    isLoading: status === 'loading',
    isAuthenticated: !!session?.user,
    login,
    logout,
  };
} 