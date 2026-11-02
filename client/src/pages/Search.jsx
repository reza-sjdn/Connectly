import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { api } from '../api/client.js';
import Avatar from '../components/Avatar.jsx';
import FollowButton from '../components/FollowButton.jsx';
import PostCard from '../components/PostCard.jsx';

export default function Search() {
  const [params] = useSearchParams();
  const q = (params.get('q') ?? '').trim();
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    setResult(null);
    setError('');
    if (q.length < 2) return;
    api(`/api/search?q=${encodeURIComponent(q)}`)
      .then(setResult)
      .catch((e) => setError(e.errors ? Object.values(e.errors).flat().join(' ') : e.message));
  }, [q]);

  const setFollowing = (id, value) =>
    setResult((r) => ({ ...r, people: r.people.map((p) => (p.id === id ? { ...p, isFollowedByMe: value } : p)) }));
  const onChanged = (id, patch) =>
    setResult((r) => ({ ...r, posts: r.posts.map((p) => (p.id === id ? { ...p, ...patch } : p)) }));
  const onDeleted = (id) => setResult((r) => ({ ...r, posts: r.posts.filter((p) => p.id !== id) }));

  if (q.length < 2) return <div className="card muted">Type at least 2 characters in the search box.</div>;
  if (error) return <div className="card error">{error}</div>;
  if (!result) return <div className="center muted">Searching…</div>;

  const nothing = !result.people.length && !result.groups.length && !result.posts.length;

  return (
    <div className="feed">
      <div className="card"><h2>Results for “{q}”</h2></div>
      {nothing && <div className="card muted">Nothing found. Try another word or a shorter one.</div>}

      {result.people.length > 0 && (
        <div className="card">
          <h3>People</h3>
          <ul className="user-list">
            {result.people.map((p) => (
              <li key={p.id}>
                <Link to={`/profile/${p.id}`} className="user-link">
                  <Avatar url={p.avatarUrl} name={`${p.firstName} ${p.lastName}`} />
                  <span>{p.firstName} {p.lastName}</span>
                </Link>
                <FollowButton userId={p.id} following={p.isFollowedByMe} onChange={(v) => setFollowing(p.id, v)} />
              </li>
            ))}
          </ul>
        </div>
      )}

      {result.groups.length > 0 && (
        <div className="card">
          <h3>Groups</h3>
          <ul className="user-list">
            {result.groups.map((g) => (
              <li key={g.id}>
                <Link to={`/groups/${g.id}`} className="user-link">
                  <span className="group-icon">{g.name[0].toUpperCase()}</span>
                  <span>
                    <span className="author-name">{g.name}</span>
                    <span className="muted small block">{g.privacy} group · {g.membersCount} {g.membersCount === 1 ? 'member' : 'members'}</span>
                  </span>
                </Link>
                {g.myState === 'Pending' && <span className="pill">Requested</span>}
              </li>
            ))}
          </ul>
        </div>
      )}

      {result.posts.length > 0 && <h3 className="section-title">Posts</h3>}
      {result.posts.map((p) => <PostCard key={p.id} post={p} onDeleted={onDeleted} onChanged={onChanged} />)}
    </div>
  );
}
