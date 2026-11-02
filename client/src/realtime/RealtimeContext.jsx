import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { API_URL, api, getAccessToken } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { describeNotification } from '../utils/notifications.js';

const RealtimeContext = createContext(null);
let toastSeq = 0;

export function RealtimeProvider({ children }) {
  const { user } = useAuth();
  const userId = user?.id;
  const location = useLocation();
  const pathRef = useRef(location.pathname);
  pathRef.current = location.pathname;

  const [connection, setConnection] = useState(null);
  const [online, setOnline] = useState(() => new Set());
  const [unreadNotifications, setUnreadNotifications] = useState(0);
  const [unreadMessages, setUnreadMessages] = useState(0);
  const [toasts, setToasts] = useState([]);
  const tracked = useRef(new Set());

  const pushToast = useCallback((toast) => {
    const id = ++toastSeq;
    setToasts((t) => [...t.slice(-2), { ...toast, id }]);
    setTimeout(() => setToasts((t) => t.filter((x) => x.id !== id)), 6000);
  }, []);

  const refreshUnread = useCallback(async () => {
    try {
      const [n, m] = await Promise.all([api('/api/notifications/unread-count'), api('/api/chat/unread-count')]);
      setUnreadNotifications(n.count);
      setUnreadMessages(m.count);
    } catch { /* ignore */ }
  }, []);

  // Ask the server which of these users are online (it only answers for related users) and remember them for resync.
  const trackPresence = useCallback(async (ids) => {
    const list = ids.filter(Boolean);
    if (list.length === 0) return;
    list.forEach((i) => tracked.current.add(i));
    try {
      const res = await api('/api/presence/online', { method: 'POST', body: { ids: list } });
      setOnline((prev) => {
        const next = new Set(prev);
        list.forEach((i) => (res.onlineIds.includes(i) ? next.add(i) : next.delete(i)));
        return next;
      });
    } catch { /* ignore */ }
  }, []);

  useEffect(() => {
    if (!userId) { setConnection(null); setOnline(new Set()); return undefined; }

    let cancelled = false;
    refreshUnread();

    const conn = new HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/app`, { accessTokenFactory: getAccessToken })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    conn.on('PresenceChanged', (id, isOnline) =>
      setOnline((prev) => {
        const next = new Set(prev);
        if (isOnline) next.add(id); else next.delete(id);
        return next;
      }));

    conn.on('NotificationReceived', (n, unread) => {
      setUnreadNotifications(unread);
      const d = describeNotification(n);
      pushToast({ text: `${d.who} ${d.text}`, to: d.to });
    });

    conn.on('MessageReceived', (m) => {
      refreshUnread();
      const fromOther = m.senderId !== userId;
      const alreadyThere = pathRef.current === `/messages/${m.senderId}`;
      if (fromOther && !alreadyThere) pushToast({ text: '💬 You have a new message', to: `/messages/${m.senderId}` });
    });

    conn.on('MessagesRead', () => refreshUnread());

    conn.onreconnected(() => {
      refreshUnread();
      trackPresence([...tracked.current]);
    });

    conn.start()
      .then(() => { if (!cancelled) setConnection(conn); })
      .catch(() => { /* stopped during negotiation (StrictMode) or server down: automatic reconnect does not apply to the first start */ });

    return () => {
      cancelled = true;
      setConnection(null);
      conn.stop();
    };
  }, [userId, pushToast, refreshUnread, trackPresence]);

  const value = useMemo(
    () => ({ connection, online, unreadNotifications, unreadMessages, refreshUnread, trackPresence }),
    [connection, online, unreadNotifications, unreadMessages, refreshUnread, trackPresence]
  );

  return (
    <RealtimeContext.Provider value={value}>
      {children}
      <div className="toasts">
        {toasts.map((t) => (
          <Link key={t.id} to={t.to} className="toast" onClick={() => setToasts((all) => all.filter((x) => x.id !== t.id))}>
            {t.text}
          </Link>
        ))}
      </div>
    </RealtimeContext.Provider>
  );
}

export const useRealtime = () => useContext(RealtimeContext);

// Subscribe to a hub event for the lifetime of the component (always calls the latest handler).
export function useHubEvent(name, handler) {
  const { connection } = useRealtime();
  const ref = useRef(handler);
  ref.current = handler;

  useEffect(() => {
    if (!connection) return undefined;
    const fn = (...args) => ref.current(...args);
    connection.on(name, fn);
    return () => connection.off(name, fn);
  }, [connection, name]);
}
