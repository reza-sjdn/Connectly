import { useCallback, useEffect, useState } from 'react';
import { api, mediaUrl } from '../api/client.js';
import { timeAgo } from '../utils/time.js';
import Avatar from './Avatar.jsx';

const STORY_MS = 5000;

export default function StoryViewer({ groups, startIndex, onClose }) {
  const [gi, setGi] = useState(startIndex);
  const [si, setSi] = useState(() => {
    const first = groups[startIndex].stories.findIndex((s) => !s.isSeenByMe);   // resume at the first unseen story
    return first === -1 ? 0 : first;
  });
  const [viewers, setViewers] = useState(null);   // null = closed
  const [error, setError] = useState('');

  const group = groups[gi];
  const story = group.stories[si];
  const paused = viewers !== null;

  const next = useCallback(() => {
    if (si < group.stories.length - 1) setSi(si + 1);
    else if (gi < groups.length - 1) { setGi(gi + 1); setSi(0); }
    else onClose();
  }, [si, gi, group, groups.length, onClose]);

  const prev = useCallback(() => {
    if (si > 0) setSi(si - 1);
    else if (gi > 0) { setGi(gi - 1); setSi(groups[gi - 1].stories.length - 1); }
  }, [si, gi, groups]);

  // Count a view (the server ignores the author and repeated views)
  useEffect(() => {
    if (!group.isMe) api(`/api/stories/${story.id}/view`, { method: 'POST' }).catch(() => {});
    setViewers(null);
    setError('');
  }, [story.id, group.isMe]);

  useEffect(() => {
    const onKey = (e) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowRight') next();
      if (e.key === 'ArrowLeft') prev();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [next, prev, onClose]);

  async function showViewers() {
    try { setViewers(await api(`/api/stories/${story.id}/viewers`)); }
    catch (e) { setError(e.message); }
  }

  async function remove() {
    if (!window.confirm('Delete this story?')) return;
    try { await api(`/api/stories/${story.id}`, { method: 'DELETE' }); onClose(); }
    catch (e) { setError(e.message); }
  }

  const name = `${group.author.firstName} ${group.author.lastName}`;

  return (
    <div className="story-overlay" onClick={onClose}>
      <div className="story-stage" onClick={(e) => e.stopPropagation()}>
        <div className="story-progress">
          {group.stories.map((s, i) => (
            <div key={s.id} className="seg">
              {i < si && <div className="fill done" />}
              {i === si && (
                <div key={story.id} className={`fill run ${paused ? 'paused' : ''}`}
                     style={{ animationDuration: `${STORY_MS}ms` }} onAnimationEnd={next} />
              )}
            </div>
          ))}
        </div>

        <header className="story-head">
          <Avatar url={group.author.avatarUrl} name={name} size={36} />
          <div>
            <strong>{name}</strong>
            <div className="small story-time">{timeAgo(story.createdAtUtc)}</div>
          </div>
          <button className="story-close" onClick={onClose} aria-label="Close">✕</button>
        </header>

        <div className="story-content" style={story.imageUrl ? undefined : { background: story.backgroundColor }}>
          {story.imageUrl
            ? <img src={mediaUrl(story.imageUrl)} alt="" />
            : <p>{story.text}</p>}
          <button className="story-nav left" onClick={prev} aria-label="Previous" />
          <button className="story-nav right" onClick={next} aria-label="Next" />
        </div>

        {group.isMe && (
          <footer className="story-foot">
            <button className="link story-link" onClick={showViewers}>👁 Seen by {story.viewsCount}</button>
            <button className="link story-link" onClick={remove}>Delete</button>
          </footer>
        )}
        {error && <div className="error story-error">{error}</div>}

        {viewers && (
          <div className="viewers">
            <div className="row between">
              <strong>Seen by {viewers.length}</strong>
              <button className="link" onClick={() => setViewers(null)}>Close</button>
            </div>
            {viewers.length === 0 && <p className="muted small">Nobody yet.</p>}
            <ul className="user-list">
              {viewers.map((v) => (
                <li key={v.user.id}>
                  <span className="user-link">
                    <Avatar url={v.user.avatarUrl} name={`${v.user.firstName} ${v.user.lastName}`} size={32} />
                    <span>{v.user.firstName} {v.user.lastName}</span>
                  </span>
                  <span className="muted small">{timeAgo(v.viewedAtUtc)}</span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}
