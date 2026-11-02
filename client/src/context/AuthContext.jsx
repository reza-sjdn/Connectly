import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { api, tokenStore } from '../api/client.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const loadProfile = useCallback(async () => {
    const profile = await api('/api/profiles/me');
    setUser(profile);
    return profile;
  }, []);

  // Restore session on first load
  useEffect(() => {
    (async () => {
      if (tokenStore.get()) {
        try { await loadProfile(); } catch { tokenStore.clear(); }
      }
      setLoading(false);
    })();
  }, [loadProfile]);

  const login = useCallback(async (email, password) => {
    const tokens = await api('/api/auth/login', { method: 'POST', body: { email, password }, auth: false });
    tokenStore.set(tokens);
    await loadProfile();
  }, [loadProfile]);

  const register = useCallback(async (form) => {
    const tokens = await api('/api/auth/register', { method: 'POST', body: form, auth: false });
    tokenStore.set(tokens);
    await loadProfile();
  }, [loadProfile]);

  const logout = useCallback(async () => {
    const session = tokenStore.get();
    try {
      if (session?.refreshToken) {
        await api('/api/auth/logout', { method: 'POST', body: { refreshToken: session.refreshToken }, auth: false });
      }
    } catch { /* best effort */ }
    tokenStore.clear();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({ user, loading, login, register, logout, refreshUser: loadProfile }),
    [user, loading, login, register, logout, loadProfile]
  );
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => useContext(AuthContext);
