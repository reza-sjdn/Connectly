import { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client.js';
import { useHubEvent } from '../realtime/RealtimeContext.jsx';
import { timeAgo } from '../utils/time.js';
import Avatar from './Avatar.jsx';

const PAGE_SIZE = 10;

export default function CommentSection({ postId, onCountChange }) {
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [text, setText] = useState('');
  const [error, setError] = useState('');

  const load = useCallback(async (p) => {
    setLoading(true);
    try {
      const res = await api(`/api/posts/${postId}/comments?page=${p}&pageSize=${PAGE_SIZE}`);
      setItems((prev) => (p === 1 ? res.items : [...prev, ...res.items]));
      setTotal(res.totalCount);
      setPage(p);
    } catch (e) { setError(e.message); }
    finally { setLoading(false); }
  }, [postId]);

  useEffect(() => { load(1); }, [load]);

  // Comments from other people arrive live. Only the post owner is pushed these, so they may delete them.
  const itemsRef = useRef(items);
  itemsRef.current = items;
  useHubEvent('CommentAdded', (pid, comment) => {
    if (pid !== postId || itemsRef.current.some((x) => x.id === comment.id)) return;
    setItems((prev) => [...prev, { ...comment, isMine: false, canDelete: true }]);
    setTotal((t) => t + 1);
  });

  async function add(e) {
    e.preventDefault();
    if (!text.trim()) return;
    setError('');
    try {
      const c = await api(`/api/posts/${postId}/comments`, { method: 'POST', body: { content: text.trim() } });
      setItems((prev) => [...prev, c]);
      setTotal((t) => t + 1);
      onCountChange(+1);
      setText('');
    } catch (err) {
      setError(err.errors ? Object.values(err.errors).flat().join(' ') : err.message);
    }
  }

  async function remove(id) {
    try {
      await api(`/api/comments/${id}`, { method: 'DELETE' });
      setItems((prev) => prev.filter((c) => c.id !== id));
      setTotal((t) => t - 1);
      onCountChange(-1);
    } catch (err) { setError(err.message); }
  }

  return (
    <div className="comments">
      {items.map((c) => (
        <div className="comment" key={c.id}>
          <Link to={`/profile/${c.author.id}`}><Avatar url={c.author.avatarUrl} name={`${c.author.firstName} ${c.author.lastName}`} size={32} /></Link>
          <div className="comment-body">
            <div className="comment-bubble">
              <Link to={`/profile/${c.author.id}`} className="author-name">{c.author.firstName} {c.author.lastName}</Link>
              <div className="pre">{c.content}</div>
            </div>
            <div className="muted small">
              {timeAgo(c.createdAtUtc)}
              {c.canDelete && <button className="link small" onClick={() => remove(c.id)}>Delete</button>}
            </div>
          </div>
        </div>
      ))}
      {items.length < total && (
        <button className="link small" disabled={loading} onClick={() => load(page + 1)}>View more comments</button>
      )}
      <form className="comment-form" onSubmit={add}>
        <input value={text} maxLength={2000} placeholder="Write a comment…" onChange={(e) => setText(e.target.value)} />
        <button className="btn-primary" disabled={!text.trim()}>Send</button>
      </form>
      {error && <div className="error">{error}</div>}
    </div>
  );
}
