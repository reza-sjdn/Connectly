import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client.js';
import Avatar from './Avatar.jsx';

const errText = (e) => (e.errors ? Object.values(e.errors).flat().join(' ') : e.message);

export function GroupMembers({ groupId, isAdmin, meId, onChanged }) {
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [error, setError] = useState('');

  const load = useCallback(async (p) => {
    const res = await api(`/api/groups/${groupId}/members?page=${p}&pageSize=20`);
    setItems((prev) => (p === 1 ? res.items : [...prev, ...res.items]));
    setTotal(res.totalCount);
    setPage(p);
  }, [groupId]);

  useEffect(() => { load(1).catch((e) => setError(errText(e))); }, [load]);

  async function act(fn) {
    setError('');
    try { await fn(); await load(1); onChanged?.(); } catch (e) { setError(errText(e)); }
  }

  const setRole = (userId, role) =>
    act(() => api(`/api/groups/${groupId}/members/${userId}/role`, { method: 'PUT', body: { role } }));
  const remove = (userId) => {
    if (!window.confirm('Remove this member from the group?')) return Promise.resolve();
    return act(() => api(`/api/groups/${groupId}/members/${userId}`, { method: 'DELETE' }));
  };

  return (
    <div className="card">
      <h3>Members ({total})</h3>
      {error && <div className="error">{error}</div>}
      <ul className="user-list">
        {items.map((m) => (
          <li key={m.user.id}>
            <Link to={`/profile/${m.user.id}`} className="user-link">
              <Avatar url={m.user.avatarUrl} name={`${m.user.firstName} ${m.user.lastName}`} />
              <span>{m.user.firstName} {m.user.lastName}</span>
              {m.role === 'Admin' && <span className="pill">Admin</span>}
            </Link>
            {isAdmin && m.user.id !== meId && (
              <span className="row">
                <button className="btn-outline" onClick={() => setRole(m.user.id, m.role === 'Admin' ? 'Member' : 'Admin')}>
                  {m.role === 'Admin' ? 'Make member' : 'Make admin'}
                </button>
                {m.role !== 'Admin' && <button className="btn-outline" onClick={() => remove(m.user.id)}>Remove</button>}
              </span>
            )}
          </li>
        ))}
      </ul>
      {items.length < total && <button className="btn-outline" onClick={() => load(page + 1)}>Load more</button>}
    </div>
  );
}

export function GroupRequests({ groupId, onChanged }) {
  const [items, setItems] = useState(null);
  const [error, setError] = useState('');

  const load = useCallback(async () => setItems(await api(`/api/groups/${groupId}/requests`)), [groupId]);
  useEffect(() => { load().catch((e) => setError(errText(e))); }, [load]);

  async function review(userId, approve) {
    setError('');
    try {
      await api(`/api/groups/${groupId}/requests/${userId}${approve ? '/approve' : ''}`, { method: approve ? 'POST' : 'DELETE' });
      await load();
      onChanged?.();
    } catch (e) { setError(errText(e)); }
  }

  if (!items) return <div className="card muted">Loading…</div>;

  return (
    <div className="card">
      <h3>Join requests ({items.length})</h3>
      {error && <div className="error">{error}</div>}
      {items.length === 0 && <p className="muted">No pending requests.</p>}
      <ul className="user-list">
        {items.map((r) => (
          <li key={r.user.id}>
            <Link to={`/profile/${r.user.id}`} className="user-link">
              <Avatar url={r.user.avatarUrl} name={`${r.user.firstName} ${r.user.lastName}`} />
              <span>{r.user.firstName} {r.user.lastName}</span>
            </Link>
            <span className="row">
              <button className="btn-primary" onClick={() => review(r.user.id, true)}>Approve</button>
              <button className="btn-outline" onClick={() => review(r.user.id, false)}>Decline</button>
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}

export function GroupSettings({ group, onSaved }) {
  const [form, setForm] = useState({ name: group.name, description: group.description ?? '', privacy: group.privacy });
  const [errors, setErrors] = useState([]);
  const [busy, setBusy] = useState(false);
  const [saved, setSaved] = useState(false);

  const onChange = (e) => { setSaved(false); setForm({ ...form, [e.target.name]: e.target.value }); };

  async function submit(e) {
    e.preventDefault();
    setBusy(true);
    setErrors([]);
    try {
      await api(`/api/groups/${group.id}`, { method: 'PUT', body: { ...form, description: form.description || null } });
      setSaved(true);
      await onSaved();
    } catch (err) {
      setErrors(err.errors ? Object.values(err.errors).flat() : [err.message]);
    } finally { setBusy(false); }
  }

  return (
    <form className="card" onSubmit={submit}>
      <h3>Group settings</h3>
      <input name="name" value={form.name} onChange={onChange} placeholder="Group name" required />
      <textarea name="description" rows={3} maxLength={1000} value={form.description} onChange={onChange} placeholder="Description" />
      <label className="muted small">Privacy
        <select name="privacy" value={form.privacy} onChange={onChange}>
          <option value="Public">Public – anyone can join and see posts</option>
          <option value="Private">Private – people must be approved by an admin</option>
        </select>
      </label>
      {errors.length > 0 && <ul className="error">{errors.map((m, i) => <li key={i}>{m}</li>)}</ul>}
      {saved && <div className="muted small">Saved.</div>}
      <button className="btn-primary" disabled={busy}>{busy ? 'Saving…' : 'Save changes'}</button>
    </form>
  );
}
