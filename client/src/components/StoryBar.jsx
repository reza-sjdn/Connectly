import { useCallback, useEffect, useState } from 'react';
import { api } from '../api/client.js';
import Avatar from './Avatar.jsx';
import StoryCreator from './StoryCreator.jsx';
import StoryViewer from './StoryViewer.jsx';

export default function StoryBar() {
  const [groups, setGroups] = useState([]);
  const [creating, setCreating] = useState(false);
  const [viewing, setViewing] = useState(null);   // index into groups

  const load = useCallback(async () => {
    try { setGroups(await api('/api/stories')); } catch { /* the bar is optional */ }
  }, []);

  useEffect(() => { load(); }, [load]);

  return (
    <div className="card story-bar">
      <button className="story-chip" onClick={() => setCreating(true)}>
        <span className="story-add">+</span>
        <span className="small">Add story</span>
      </button>

      {groups.map((g, i) => (
        <button key={g.author.id} className="story-chip" onClick={() => setViewing(i)}>
          <span className={`story-ring ${g.hasUnseen ? 'unseen' : ''}`}>
            <Avatar url={g.author.avatarUrl} name={`${g.author.firstName} ${g.author.lastName}`} size={52} />
          </span>
          <span className="small">{g.isMe ? 'Your story' : g.author.firstName}</span>
        </button>
      ))}

      {creating && <StoryCreator onClose={() => setCreating(false)} onCreated={() => { setCreating(false); load(); }} />}
      {viewing !== null && <StoryViewer groups={groups} startIndex={viewing} onClose={() => { setViewing(null); load(); }} />}
    </div>
  );
}
