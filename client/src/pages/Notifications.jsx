import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client.js';
import Avatar from '../components/Avatar.jsx';
import { useHubEvent, useRealtime } from '../realtime/RealtimeContext.jsx';
import { describeNotification } from '../utils/notifications.js';
import { timeAgo } from '../utils/time.js';

export default function Notifications() {
  const navigate = useNavigate();
  const { refreshUnread } = useRealtime();
  const [items, setItems] = useState([]);
  const [cursor, setCursor] = useState(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async (cur) => {
    setLoading(true);
    try {
      const q = new URLSearchParams({ pageSize: '20' });
      if (cur) q.set('cursor', cur);
      const res = await api(`/api/notifications?${q}`);
      setItems((prev) => (cur ? [...prev, ...res.items] : res.items));
      setCursor(res.nextCursor);
    } finally { setLoading(false); }
  }, []);

  useEffect(() => { load(null); }, [load]);

  // New notification while this page is open: show it at the top immediately.
  useHubEvent('NotificationReceived', (n) => setItems((prev) => (prev.some((x) => x.id === n.id) ? prev : [n, ...prev])));

  async function open(n) {
    const { to } = describeNotification(n);
    if (!n.isRead) {
      setItems((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
      await api(`/api/notifications/${n.id}/read`, { method: 'POST' }).catch(() => {});
      refreshUnread();
    }
    navigate(to);
  }

  async function markAll() {
    await api('/api/notifications/read-all', { method: 'POST' });
    setItems((prev) => prev.map((x) => ({ ...x, isRead: true })));
    refreshUnread();
  }

  return (
    <div className="card">
      <div className="row between">
        <h2>Notifications</h2>
        <button className="link" onClick={markAll}>Mark all as read</button>
      </div>
      {!loading && items.length === 0 && <p className="muted">Nothing yet. Likes, comments and new followers show up here.</p>}
      <ul className="notif-list">
        {items.map((n) => {
          const d = describeNotification(n);
          return (
            <li key={n.id} className={n.isRead ? '' : 'unread'} onClick={() => open(n)}>
              <Avatar url={n.actor.avatarUrl} name={d.who} />
              <div>
                <div><strong>{d.who}</strong> {d.text}</div>
                <div className="muted small">{timeAgo(n.createdAtUtc)}</div>
              </div>
              {!n.isRead && <span className="dot" />}
            </li>
          );
        })}
      </ul>
      {cursor && <button className="btn-outline" disabled={loading} onClick={() => load(cursor)}>Load more</button>}
    </div>
  );
}
