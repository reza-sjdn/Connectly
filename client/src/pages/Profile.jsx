import { useCallback, useEffect, useRef, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, mediaUrl } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import Avatar from '../components/Avatar.jsx';
import EditProfileForm from '../components/EditProfileForm.jsx';
import FollowButton from '../components/FollowButton.jsx';
import Feed from '../components/Feed.jsx';
import FollowList from '../components/FollowList.jsx';
import { useRealtime } from '../realtime/RealtimeContext.jsx';

export default function Profile() {
  const { id } = useParams();
  const { user: me, refreshUser } = useAuth();
  const { online, trackPresence } = useRealtime();
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState('');
  const [editing, setEditing] = useState(false);
  const [tab, setTab] = useState('posts');
  const avatarInput = useRef(null);
  const coverInput = useRef(null);

  const load = useCallback(async () => {
    try { setProfile(await api(`/api/profiles/${id}`)); setError(''); }
    catch (e) { setError(e.message); }
  }, [id]);

  useEffect(() => { if (id !== me.id) trackPresence([id]); }, [id, me.id, trackPresence]);

  useEffect(() => { setProfile(null); setEditing(false); setTab('posts'); load(); }, [load]);

  async function upload(kind, file) {
    if (!file) return;
    const body = new FormData();
    body.append('file', file);
    try {
      await api(`/api/profiles/me/${kind}`, { method: 'POST', body });
      await Promise.all([load(), refreshUser()]);
      setError('');
    } catch (e) {
      setError(e.errors ? Object.values(e.errors).flat().join(' ') : e.message);
    }
  }

  async function onSaved() {
    await Promise.all([load(), refreshUser()]);
    setEditing(false);
  }

  function onFollowChange(value) {
    setProfile((p) => ({ ...p, isFollowedByMe: value, followersCount: p.followersCount + (value ? 1 : -1) }));
  }

  if (error && !profile) return <div className="card error">{error}</div>;
  if (!profile) return <div className="center">Loading…</div>;

  const fullName = `${profile.firstName} ${profile.lastName}`;
  const cover = mediaUrl(profile.coverUrl);

  return (
    <div className="profile">
      <div className="cover" style={cover ? { backgroundImage: `url(${cover})` } : undefined}>
        {profile.isMe && (
          <>
            <button className="btn-outline cover-btn" onClick={() => coverInput.current.click()}>Change cover</button>
            <input ref={coverInput} type="file" hidden accept="image/jpeg,image/png,image/webp"
                   onChange={(e) => { upload('cover', e.target.files[0]); e.target.value = ''; }} />
          </>
        )}
      </div>

      <div className="profile-head">
        <div className="avatar-wrap">
          <Avatar url={profile.avatarUrl} name={fullName} size={120} online={!profile.isMe && online.has(profile.id)} />
          {profile.isMe && (
            <>
              <button className="btn-outline avatar-btn" onClick={() => avatarInput.current.click()}>Change</button>
              <input ref={avatarInput} type="file" hidden accept="image/jpeg,image/png,image/webp"
                     onChange={(e) => { upload('avatar', e.target.files[0]); e.target.value = ''; }} />
            </>
          )}
        </div>
        <div className="profile-info">
          <h2>{fullName}</h2>
          <p className="muted">{profile.followersCount} followers · {profile.followingCount} following</p>
          {profile.bio && <p>{profile.bio}</p>}
        </div>
        <div>
          {profile.isMe
            ? <button className="btn-outline" onClick={() => setEditing(!editing)}>Edit profile</button>
            : (
              <div className="row">
                <FollowButton userId={profile.id} following={profile.isFollowedByMe} onChange={onFollowChange} />
                <Link className="btn-outline msg-link" to={`/messages/${profile.id}`}>Message</Link>
              </div>
            )}
        </div>
      </div>

      {error && <div className="error">{error}</div>}
      {editing && <EditProfileForm initial={me} onSaved={onSaved} onCancel={() => setEditing(false)} />}

      <div className="card tabs-card">
        <div className="tabs">
          <button className={tab === 'posts' ? 'active' : ''} onClick={() => setTab('posts')}>Posts</button>
          <button className={tab === 'followers' ? 'active' : ''} onClick={() => setTab('followers')}>Followers</button>
          <button className={tab === 'following' ? 'active' : ''} onClick={() => setTab('following')}>Following</button>
        </div>
      </div>

      {tab === 'posts'
        ? <Feed key={profile.id} url={`/api/users/${profile.id}/posts`} composer={profile.isMe} emptyText="No posts yet." />
        : <div className="card"><FollowList key={`${profile.id}-${tab}`} userId={profile.id} direction={tab} /></div>}
    </div>
  );
}
