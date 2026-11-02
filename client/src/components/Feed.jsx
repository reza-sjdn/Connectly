import { useCallback, useEffect, useState } from 'react';
import { api } from '../api/client.js';
import { useHubEvent } from '../realtime/RealtimeContext.jsx';
import PostCard from './PostCard.jsx';
import PostComposer from './PostComposer.jsx';

// url: '/api/posts/feed' or `/api/users/${id}/posts`
export default function Feed({ url, composer = false, groupId, emptyText = 'No posts yet.' }) {
  const [posts, setPosts] = useState([]);
  const [cursor, setCursor] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(async (cur) => {
    setLoading(true);
    try {
      const q = new URLSearchParams({ pageSize: '10' });
      if (cur) q.set('cursor', cur);
      const res = await api(`${url}?${q}`);
      setPosts((prev) => (cur ? [...prev, ...res.items] : res.items));
      setCursor(res.nextCursor);
      setError('');
    } catch (e) { setError(e.message); }
    finally { setLoading(false); }
  }, [url]);

  useEffect(() => { setPosts([]); setCursor(null); load(null); }, [load]);

  const onCreated = (post) => setPosts((prev) => [post, ...prev]);
  const onDeleted = (id) => setPosts((prev) => prev.filter((p) => p.id !== id));
  const onChanged = (id, patch) => setPosts((prev) => prev.map((p) => (p.id === id ? { ...p, ...patch } : p)));

  useHubEvent('PostStatsChanged', (postId, likesCount, commentsCount) =>
    onChanged(postId, { likesCount, commentsCount }));

  return (
    <div className="feed">
      {composer && <PostComposer onCreated={onCreated} groupId={groupId} />}
      {error && <div className="card error">{error}</div>}
      {posts.map((p) => <PostCard key={p.id} post={p} onDeleted={onDeleted} onChanged={onChanged} />)}
      {!loading && posts.length === 0 && !error && <div className="card muted">{emptyText}</div>}
      {loading && <div className="center muted">Loading…</div>}
      {!loading && cursor && <button className="btn-outline load-more" onClick={() => load(cursor)}>Load more</button>}
    </div>
  );
}
