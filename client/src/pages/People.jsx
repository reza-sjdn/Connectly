import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client.js';
import Avatar from '../components/Avatar.jsx';
import FollowButton from '../components/FollowButton.jsx';

export default function People() {
  const [people, setPeople] = useState(null);

  useEffect(() => { api('/api/profiles/suggestions?take=30').then(setPeople); }, []);

  const setFollowing = (id, value) =>
    setPeople((prev) => prev.map((p) => (p.id === id ? { ...p, isFollowedByMe: value } : p)));

  if (!people) return <div className="center">Loading…</div>;

  return (
    <div className="card">
      <h2>People you may know</h2>
      {people.length === 0 && <p className="muted">Nobody new yet. Register another account to try following.</p>}
      <ul className="user-list">
        {people.map((p) => (
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
  );
}
