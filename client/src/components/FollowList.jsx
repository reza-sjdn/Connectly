import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import Avatar from './Avatar.jsx';
import FollowButton from './FollowButton.jsx';

const PAGE_SIZE = 20;

// direction: 'followers' | 'following'
export default function FollowList({ userId, direction }) {
  const { user: me } = useAuth();
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async (p) => {
    setLoading(true);
    try {
      const res = await api(`/api/users/${userId}/${direction}?page=${p}&pageSize=${PAGE_SIZE}`);
      setItems((prev) => (p === 1 ? res.items : [...prev, ...res.items]));
      setTotal(res.totalCount);
      setPage(p);
    } finally { setLoading(false); }
  }, [userId, direction]);

  useEffect(() => { setItems([]); load(1); }, [load]);

  const setFollowing = (id, value) =>
    setItems((prev) => prev.map((i) => (i.id === id ? { ...i, isFollowedByMe: value } : i)));

  if (!loading && items.length === 0) {
    return <p className="muted">{direction === 'followers' ? 'No followers yet.' : 'Not following anyone yet.'}</p>;
  }

  return (
    <div>
      <ul className="user-list">
        {items.map((u) => (
          <li key={u.id}>
            <Link to={`/profile/${u.id}`} className="user-link">
              <Avatar url={u.avatarUrl} name={`${u.firstName} ${u.lastName}`} />
              <span>{u.firstName} {u.lastName}</span>
            </Link>
            {u.id !== me.id && (
              <FollowButton userId={u.id} following={u.isFollowedByMe} onChange={(v) => setFollowing(u.id, v)} />
            )}
          </li>
        ))}
      </ul>
      {items.length < total && (
        <button className="btn-outline" disabled={loading} onClick={() => load(page + 1)}>Load more</button>
      )}
    </div>
  );
}
