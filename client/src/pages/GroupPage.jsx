import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import Feed from '../components/Feed.jsx';
import GroupChat from '../components/GroupChat.jsx';
import { GroupMembers, GroupRequests, GroupSettings } from '../components/GroupPanels.jsx';

export default function GroupPage() {
  const { id } = useParams();
  const { user: me } = useAuth();
  const [group, setGroup] = useState(null);
  const [error, setError] = useState('');
  const [tab, setTab] = useState('posts');
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    try { setGroup(await api(`/api/groups/${id}`)); setError(''); }
    catch (e) { setError(e.status === 404 ? 'This group does not exist.' : e.message); }
  }, [id]);

  useEffect(() => { setGroup(null); setTab('posts'); load(); }, [load]);

  if (error && !group) return <div className="card muted">{error}</div>;
  if (!group) return <div className="center muted">Loading…</div>;

  const isAdmin = group.myState === 'Admin';
  const isMember = isAdmin || group.myState === 'Member';
  const canView = group.privacy === 'Public' || isMember;

  async function join() {
    setBusy(true);
    try { setGroup(await api(`/api/groups/${id}/join`, { method: 'POST' })); }
    catch (e) { setError(e.message); }
    finally { setBusy(false); }
  }

  async function leave() {
    const pending = group.myState === 'Pending';
    if (!pending && !window.confirm('Leave this group?')) return;
    setBusy(true);
    try {
      await api(`/api/groups/${id}/membership`, { method: 'DELETE' });
      setTab('posts');
      await load();
    } catch (e) {
      setError(e.errors ? Object.values(e.errors).flat().join(' ') : e.message);
    } finally { setBusy(false); }
  }

  const tabs = [['posts', 'Posts'], ['members', 'Members']];
  if (isMember) tabs.push(['chat', 'Chat']);
  if (isAdmin) tabs.push(['requests', 'Requests'], ['settings', 'Settings']);

  return (
    <div className="feed">
      <div className="card">
        <div className="row between">
          <div>
            <h2>{group.name}</h2>
            <p className="muted small">
              {group.privacy} group · {group.membersCount} {group.membersCount === 1 ? 'member' : 'members'}
            </p>
          </div>
          <div>
            {group.myState === 'None' && (
              <button className="btn-primary" disabled={busy} onClick={join}>
                {group.privacy === 'Private' ? 'Request to join' : 'Join group'}
              </button>
            )}
            {group.myState === 'Pending' && <button className="btn-outline" disabled={busy} onClick={leave}>Requested · Cancel</button>}
            {isMember && <button className="btn-outline" disabled={busy} onClick={leave}>Leave group</button>}
          </div>
        </div>
        {group.description && <p className="pre">{group.description}</p>}
        {error && <div className="error">{error}</div>}
      </div>

      {!canView ? (
        <div className="card muted">
          This is a private group. {group.myState === 'Pending'
            ? 'Your request is waiting for an admin to approve it.'
            : 'Request to join to see its posts and members.'}
        </div>
      ) : (
        <>
          <div className="card tabs-card">
            <div className="tabs">
              {tabs.map(([key, label]) => (
                <button key={key} className={tab === key ? 'active' : ''} onClick={() => setTab(key)}>{label}</button>
              ))}
            </div>
          </div>

          {tab === 'posts' && (
            <Feed key={id} url={`/api/groups/${id}/posts`} composer={isMember} groupId={id} emptyText="No posts in this group yet." />
          )}
          {tab === 'members' && <GroupMembers groupId={id} isAdmin={isAdmin} meId={me.id} onChanged={load} />}
          {tab === 'chat' && isMember && <GroupChat groupId={id} />}
          {tab === 'requests' && isAdmin && <GroupRequests groupId={id} onChanged={load} />}
          {tab === 'settings' && isAdmin && <GroupSettings group={group} onSaved={load} />}
        </>
      )}
    </div>
  );
}
