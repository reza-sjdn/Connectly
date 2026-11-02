import { useState } from 'react';
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useRealtime } from '../realtime/RealtimeContext.jsx';
import Avatar from './Avatar.jsx';

function Badge({ count }) {
  return count > 0 ? <span className="badge">{count > 99 ? '99+' : count}</span> : null;
}

export default function Layout() {
  const { user, logout } = useAuth();
  const { unreadNotifications, unreadMessages } = useRealtime();
  const navigate = useNavigate();
  const [q, setQ] = useState('');
  const fullName = `${user.firstName} ${user.lastName}`;

  function search(e) {
    e.preventDefault();
    if (q.trim().length >= 2) navigate(`/search?q=${encodeURIComponent(q.trim())}`);
  }

  return (
    <div>
      <header className="topbar">
        <Link to="/" className="logo">connectly</Link>
        <form className="search-box" onSubmit={search}>
          <input value={q} onChange={(e) => setQ(e.target.value)} placeholder="Search people, groups, posts" maxLength={100} />
        </form>
        <nav>
          <NavLink to="/" end>Home</NavLink>
          <NavLink to="/people">People</NavLink>
          <NavLink to="/groups">Groups</NavLink>
          <NavLink to="/messages">Messages <Badge count={unreadMessages} /></NavLink>
          <NavLink to="/notifications">Notifications <Badge count={unreadNotifications} /></NavLink>
        </nav>
        <div className="me">
          <Link to={`/profile/${user.id}`} className="user-link">
            <Avatar url={user.avatarUrl} name={fullName} size={32} />
            <span>{user.firstName}</span>
          </Link>
          <button className="link" onClick={logout}>Log out</button>
        </div>
      </header>
      <main className="content"><Outlet /></main>
    </div>
  );
}
