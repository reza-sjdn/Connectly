import { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useHubEvent, useRealtime } from '../realtime/RealtimeContext.jsx';
import { timeAgo } from '../utils/time.js';
import Avatar from './Avatar.jsx';

const TYPING_THROTTLE_MS = 2000;
const TYPING_IDLE_MS = 3000;
const TYPING_EXPIRE_MS = 4000;

export default function ChatThread({ otherId, onActivity }) {
  const { user: me } = useAuth();
  const { connection, online, trackPresence, refreshUnread } = useRealtime();

  const [other, setOther] = useState(null);
  const [messages, setMessages] = useState([]);   // oldest -> newest
  const [cursor, setCursor] = useState(null);     // cursor for OLDER messages
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [text, setText] = useState('');
  const [peerTyping, setPeerTyping] = useState(false);

  const bottomRef = useRef(null);
  const stickToBottom = useRef(true);
  const lastTypingSent = useRef(0);
  const idleTimer = useRef(null);
  const expireTimer = useRef(null);

  const markRead = useCallback(async () => {
    try { await api(`/api/chat/with/${otherId}/read`, { method: 'POST' }); refreshUnread(); onActivity?.(); } catch { /* ignore */ }
  }, [otherId, refreshUnread, onActivity]);

  // Initial load
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [profile, page] = await Promise.all([
          api(`/api/profiles/${otherId}`),
          api(`/api/chat/with/${otherId}/messages?pageSize=30`),
        ]);
        if (cancelled) return;
        setOther(profile);
        setMessages([...page.items].reverse());
        setCursor(page.nextCursor);
        trackPresence([otherId]);
        markRead();
      } catch (e) { if (!cancelled) setError(e.message); }
      finally { if (!cancelled) setLoading(false); }
    })();
    return () => { cancelled = true; clearTimeout(idleTimer.current); clearTimeout(expireTimer.current); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [otherId]);

  useEffect(() => {
    if (stickToBottom.current) bottomRef.current?.scrollIntoView({ block: 'end' });
  }, [messages, peerTyping]);

  const addMessage = useCallback((m) => {
    stickToBottom.current = true;
    setMessages((prev) => (prev.some((x) => x.id === m.id) ? prev : [...prev, m]));
  }, []);

  async function loadOlder() {
    const page = await api(`/api/chat/with/${otherId}/messages?pageSize=30&cursor=${encodeURIComponent(cursor)}`);
    stickToBottom.current = false;
    setMessages((prev) => [...[...page.items].reverse(), ...prev]);
    setCursor(page.nextCursor);
  }

  // ---- live events ----
  useHubEvent('MessageReceived', (m) => {
    const peer = m.senderId === me.id ? m.recipientId : m.senderId;
    if (peer !== otherId) return;
    addMessage(m);
    if (m.senderId === otherId) {
      setPeerTyping(false);
      markRead();
    }
  });

  useHubEvent('MessagesRead', (readerId, readAt) => {
    if (readerId !== otherId) return;
    setMessages((prev) => prev.map((m) => (m.senderId === me.id && !m.readAtUtc ? { ...m, readAtUtc: readAt } : m)));
  });

  useHubEvent('Typing', (userId, isTyping) => {
    if (userId !== otherId) return;
    setPeerTyping(isTyping);
    clearTimeout(expireTimer.current);
    if (isTyping) expireTimer.current = setTimeout(() => setPeerTyping(false), TYPING_EXPIRE_MS);   // in case "stopped" is lost
  });

  // ---- sending + typing indicator ----
  function stopTyping() {
    clearTimeout(idleTimer.current);
    if (lastTypingSent.current !== 0) {
      lastTypingSent.current = 0;
      connection?.invoke('Typing', otherId, false).catch(() => {});
    }
  }

  function onType(value) {
    setText(value);
    if (!value) { stopTyping(); return; }
    const now = Date.now();
    if (connection && now - lastTypingSent.current > TYPING_THROTTLE_MS) {
      lastTypingSent.current = now;
      connection.invoke('Typing', otherId, true).catch(() => {});
    }
    clearTimeout(idleTimer.current);
    idleTimer.current = setTimeout(stopTyping, TYPING_IDLE_MS);
  }

  async function send(e) {
    e.preventDefault();
    const content = text.trim();
    if (!content) return;
    setText('');
    stopTyping();
    setError('');
    try {
      const m = await api(`/api/chat/with/${otherId}/messages`, { method: 'POST', body: { content } });
      addMessage(m);
      onActivity?.();
    } catch (err) {
      setText(content);
      setError(err.errors ? Object.values(err.errors).flat().join(' ') : err.message);
    }
  }

  if (error && !other) return <p className="error">{error}</p>;
  if (loading || !other) return <div className="center muted">Loading…</div>;

  const name = `${other.firstName} ${other.lastName}`;
  const lastMineIndex = messages.map((m) => m.senderId === me.id).lastIndexOf(true);
  const isOnline = online.has(otherId);

  return (
    <div className="thread">
      <header className="thread-head">
        <Link to={`/profile/${other.id}`} className="user-link">
          <Avatar url={other.avatarUrl} name={name} online={isOnline} />
          <span>
            <span className="author-name">{name}</span>
            <span className="muted small block">{peerTyping ? 'typing…' : isOnline ? 'Online' : 'Offline'}</span>
          </span>
        </Link>
      </header>

      <div className="thread-body">
        {cursor && <button className="link small" onClick={loadOlder}>Load older messages</button>}
        {messages.length === 0 && <p className="muted small center">Say hi to {other.firstName} 👋</p>}
        {messages.map((m, i) => {
          const mine = m.senderId === me.id;
          return (
            <div key={m.id} className={`bubble-row ${mine ? 'mine' : 'theirs'}`}>
              <div className="bubble" title={new Date(/[zZ]$/.test(m.createdAtUtc) ? m.createdAtUtc : `${m.createdAtUtc}Z`).toLocaleString()}>
                {m.content}
              </div>
              {mine && i === lastMineIndex && (
                <div className="muted small">{m.readAtUtc ? `Seen ${timeAgo(m.readAtUtc)}` : 'Sent'}</div>
              )}
            </div>
          );
        })}
        {peerTyping && <div className="bubble-row theirs"><div className="bubble typing"><span /><span /><span /></div></div>}
        <div ref={bottomRef} />
      </div>

      {error && <div className="error">{error}</div>}
      <form className="thread-form" onSubmit={send}>
        <input value={text} maxLength={2000} placeholder="Aa" onChange={(e) => onType(e.target.value)} onBlur={stopTyping} />
        <button className="btn-primary" disabled={!text.trim()}>Send</button>
      </form>
    </div>
  );
}
