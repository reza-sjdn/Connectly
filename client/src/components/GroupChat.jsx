import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useHubEvent } from '../realtime/RealtimeContext.jsx';
import { timeAgo } from '../utils/time.js';

export default function GroupChat({ groupId }) {
  const { user: me } = useAuth();
  const [messages, setMessages] = useState([]);   // oldest -> newest
  const [cursor, setCursor] = useState(null);
  const [loading, setLoading] = useState(true);
  const [text, setText] = useState('');
  const [error, setError] = useState('');
  const bottomRef = useRef(null);
  const stick = useRef(true);

  useEffect(() => {
    let cancelled = false;
    api(`/api/groups/${groupId}/messages?pageSize=30`)
      .then((page) => { if (!cancelled) { setMessages([...page.items].reverse()); setCursor(page.nextCursor); } })
      .catch((e) => { if (!cancelled) setError(e.message); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [groupId]);

  useEffect(() => {
    if (stick.current) bottomRef.current?.scrollIntoView({ block: 'end' });
  }, [messages]);

  const add = (m) => {
    stick.current = true;
    setMessages((prev) => (prev.some((x) => x.id === m.id) ? prev : [...prev, m]));
  };

  useHubEvent('GroupMessageReceived', (m) => { if (m.groupId === groupId) add(m); });

  async function loadOlder() {
    const page = await api(`/api/groups/${groupId}/messages?pageSize=30&cursor=${encodeURIComponent(cursor)}`);
    stick.current = false;
    setMessages((prev) => [...[...page.items].reverse(), ...prev]);
    setCursor(page.nextCursor);
  }

  async function send(e) {
    e.preventDefault();
    const content = text.trim();
    if (!content) return;
    setText('');
    setError('');
    try { add(await api(`/api/groups/${groupId}/messages`, { method: 'POST', body: { content } })); }
    catch (err) { setText(content); setError(err.message); }
  }

  if (loading) return <div className="card muted">Loading…</div>;

  return (
    <div className="card chat-pane">
      <div className="thread" style={{ height: 480 }}>
        <div className="thread-body">
          {cursor && <button className="link small" onClick={loadOlder}>Load older messages</button>}
          {messages.length === 0 && <p className="muted small center">No messages yet. Say hi to the group 👋</p>}
          {messages.map((m) => {
            const mine = m.sender.id === me.id;
            return (
              <div key={m.id} className={`bubble-row ${mine ? 'mine' : 'theirs'}`}>
                {!mine && (
                  <Link to={`/profile/${m.sender.id}`} className="muted small">
                    {m.sender.firstName} {m.sender.lastName} · {timeAgo(m.createdAtUtc)}
                  </Link>
                )}
                <div className="bubble">{m.content}</div>
              </div>
            );
          })}
          <div ref={bottomRef} />
        </div>
        {error && <div className="error">{error}</div>}
        <form className="thread-form" onSubmit={send}>
          <input value={text} maxLength={2000} placeholder="Message the group" onChange={(e) => setText(e.target.value)} />
          <button className="btn-primary" disabled={!text.trim()}>Send</button>
        </form>
      </div>
    </div>
  );
}
