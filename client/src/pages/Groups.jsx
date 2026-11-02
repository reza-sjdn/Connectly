import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api } from '../api/client.js';

const errText = (e) => (e.errors ? Object.values(e.errors).flat().join(' ') : e.message);

function CreateGroup({ onCreated }) {
  const [form, setForm] = useState({ name: '', description: '', privacy: 'Public' });
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const onChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  async function submit(e) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      const g = await api('/api/groups', { method: 'POST', body: { ...form, description: form.description || null } });
      onCreated(g);
    } catch (err) { setError(errText(err)); }
    finally { setBusy(false); }
  }

  return (
    <form className="card" onSubmit={submit}>
      <h3>Create a group</h3>
      <input name="name" placeholder="Group name" value={form.name} onChange={onChange} required minLength={3} maxLength={100} />
      <textarea name="description" rows={2} placeholder="What is this group about? (optional)" maxLength={1000} value={form.description} onChange={onChange} />
      <select name="privacy" value={form.privacy} onChange={onChange}>
        <option value="Public">Public – anyone can join and see posts</option>
        <option value="Private">Private – admin approval required</option>
      </select>
      {error && <div className="error">{error}</div>}
      <button className="btn-primary" disabled={busy}>{busy ? 'Creating…' : 'Create group'}</button>
    </form>
  );
}

export default function Groups() {
  const navigate = useNavigate();
  const [scope, setScope] = useState('Mine');
  const [groups, setGroups] = useState(null);
  const [showCreate, setShowCreate] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setGroups(null);
    try { setGroups(await api(`/api/groups?scope=${scope}`)); } catch (e) { setError(errText(e)); }
  }, [scope]);

  useEffect(() => { load(); }, [load]);

  async function join(g) {
    try {
      const res = await api(`/api/groups/${g.id}/join`, { method: 'POST' });
      if (res.myState === 'Member' || res.myState === 'Admin') navigate(`/groups/${g.id}`);
      else setGroups((prev) => prev.map((x) => (x.id === g.id ? { ...x, myState: res.myState } : x)));
    } catch (e) { setError(errText(e)); }
  }

  return (
    <div className="feed">
      <div className="card">
        <div className="row between">
          <h2>Groups</h2>
          <button className="btn-primary" onClick={() => setShowCreate((v) => !v)}>{showCreate ? 'Cancel' : 'Create group'}</button>
        </div>
        <div className="tabs">
          <button className={scope === 'Mine' ? 'active' : ''} onClick={() => setScope('Mine')}>My groups</button>
          <button className={scope === 'Discover' ? 'active' : ''} onClick={() => setScope('Discover')}>Discover</button>
        </div>
      </div>

      {showCreate && <CreateGroup onCreated={(g) => navigate(`/groups/${g.id}`)} />}
      {error && <div className="card error">{error}</div>}

      <div className="card">
        {!groups && <p className="muted">Loading…</p>}
        {groups && groups.length === 0 && (
          <p className="muted">{scope === 'Mine' ? 'You have not joined any groups yet. Try Discover.' : 'No other groups yet. Create the first one!'}</p>
        )}
        <ul className="user-list">
          {groups?.map((g) => (
            <li key={g.id}>
              <Link to={`/groups/${g.id}`} className="user-link">
                <span className="group-icon">{g.name[0].toUpperCase()}</span>
                <span>
                  <span className="author-name">{g.name}</span>
                  <span className="muted small block">{g.privacy} group · {g.membersCount} {g.membersCount === 1 ? 'member' : 'members'}</span>
                </span>
              </Link>
              {scope === 'Discover' && (
                g.myState === 'Pending'
                  ? <span className="pill">Requested</span>
                  : <button className="btn-primary" onClick={() => join(g)}>{g.privacy === 'Private' ? 'Request to join' : 'Join'}</button>
              )}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
