export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080';
const STORAGE_KEY = 'connectly.auth';

export const tokenStore = {
  get() {
    try { return JSON.parse(localStorage.getItem(STORAGE_KEY)); } catch { return null; }
  },
  set(value) { localStorage.setItem(STORAGE_KEY, JSON.stringify(value)); },
  clear() { localStorage.removeItem(STORAGE_KEY); },
};

export class ApiError extends Error {
  constructor(status, problem) {
    super(problem?.detail || problem?.title || 'Request failed');
    this.status = status;
    this.errors = problem?.errors ?? null; // { field: [messages] }
  }
}

// Single-flight refresh: parallel 401s share one refresh call (refresh tokens rotate, so this matters).
let refreshing = null;
function refreshSession() {
  const session = tokenStore.get();
  if (!session?.refreshToken) return Promise.reject(new Error('No session'));

  refreshing ??= fetch(`${API_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: session.refreshToken }),
  })
    .then(async (res) => {
      if (!res.ok) { tokenStore.clear(); throw new Error('Session expired'); }
      tokenStore.set(await res.json());
    })
    .finally(() => { refreshing = null; });

  return refreshing;
}

export async function api(path, { method = 'GET', body, auth = true, retry = true } = {}) {
  const headers = {};
  const isForm = body instanceof FormData;
  if (body !== undefined && !isForm) headers['Content-Type'] = 'application/json';

  const session = tokenStore.get();
  if (auth && session?.accessToken) headers.Authorization = `Bearer ${session.accessToken}`;

  const res = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : isForm ? body : JSON.stringify(body),
  });

  if (res.status === 401 && auth && retry && session?.refreshToken) {
    try { await refreshSession(); } catch { throw new ApiError(401, { detail: 'Session expired' }); }
    return api(path, { method, body, auth, retry: false });
  }

  if (res.status === 204) return null;
  const data = await res.json().catch(() => null);
  if (!res.ok) throw new ApiError(res.status, data);
  return data;
}

export const mediaUrl = (relative) => (relative ? `${API_URL}${relative}` : null);

// Used by SignalR (accessTokenFactory): returns a valid access token, refreshing it first if it is about to expire.
export async function getAccessToken() {
  let session = tokenStore.get();
  if (!session) return '';
  const expiresAt = new Date(/[zZ]$/.test(session.accessTokenExpiresAtUtc) ? session.accessTokenExpiresAtUtc : `${session.accessTokenExpiresAtUtc}Z`).getTime();
  if (expiresAt - Date.now() < 30_000) {
    try { await refreshSession(); session = tokenStore.get(); } catch { return ''; }
  }
  return session?.accessToken ?? '';
}
