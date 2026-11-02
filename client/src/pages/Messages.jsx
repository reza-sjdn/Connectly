import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import Avatar from '../components/Avatar.jsx';
import ChatThread from '../components/ChatThread.jsx';
import { useHubEvent, useRealtime } from '../realtime/RealtimeContext.jsx';
import { timeAgo } from '../utils/time.js';

export default function Messages() {
  const { userId } = useParams();
  const { online, trackPresence } = useRealtime();
  const [conversations, setConversations] = useState([]);

  const loadConversations = useCallback(async () => {
    try {
      const list = await api('/api/chat/conversations');
      setConversations(list);
      trackPresence(list.map((c) => c.otherUser.id));
    } catch { /* ignore */ }
  }, [trackPresence]);

  useEffect(() => { loadConversations(); }, [loadConversations]);
  useHubEvent('MessageReceived', () => loadConversations());
  useHubEvent('MessagesRead', () => loadConversations());

  return (
    <div className="chat-layout">
      <aside className="card chat-list">
        <h2>Chats</h2>
        {conversations.length === 0 && <p className="muted small">No conversations yet. Open someone's profile and press Message.</p>}
        {conversations.map((c) => {
          const name = `${c.otherUser.firstName} ${c.otherUser.lastName}`;
          return (
            <Link key={c.id} to={`/messages/${c.otherUser.id}`}
                  className={`chat-item ${userId === c.otherUser.id ? 'active' : ''}`}>
              <Avatar url={c.otherUser.avatarUrl} name={name} online={online.has(c.otherUser.id)} />
              <div className="chat-item-body">
                <div className="chat-item-top">
                  <strong>{name}</strong>
                  <span className="muted small">{timeAgo(c.lastMessageAtUtc)}</span>
                </div>
                <div className={`chat-preview ${c.unreadCount > 0 ? 'unread' : 'muted'}`}>{c.lastMessage}</div>
              </div>
              {c.unreadCount > 0 && <span className="badge">{c.unreadCount}</span>}
            </Link>
          );
        })}
      </aside>

      <section className="card chat-pane">
        {userId
          ? <ChatThread key={userId} otherId={userId} onActivity={loadConversations} />
          : <p className="muted">Select a conversation or start one from someone's profile.</p>}
      </section>
    </div>
  );
}
