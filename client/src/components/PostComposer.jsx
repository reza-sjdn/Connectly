import { useEffect, useRef, useState } from 'react';
import { api } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import Avatar from './Avatar.jsx';

export default function PostComposer({ onCreated, groupId }) {
  const { user } = useAuth();
  const [text, setText] = useState('');
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState(null);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const fileInput = useRef(null);

  useEffect(() => {
    if (!file) { setPreview(null); return; }
    const url = URL.createObjectURL(file);
    setPreview(url);
    return () => URL.revokeObjectURL(url);
  }, [file]);

  async function submit(e) {
    e.preventDefault();
    if (!text.trim() && !file) return;
    setBusy(true);
    setError('');
    try {
      const body = new FormData();
      if (text.trim()) body.append('content', text.trim());
      if (file) body.append('image', file);
      if (groupId) body.append('groupId', groupId);
      const post = await api('/api/posts', { method: 'POST', body });
      onCreated(post);
      setText('');
      setFile(null);
    } catch (err) {
      setError(err.errors ? Object.values(err.errors).flat().join(' ') : err.message);
    } finally { setBusy(false); }
  }

  return (
    <form className="card composer" onSubmit={submit}>
      <div className="composer-row">
        <Avatar url={user.avatarUrl} name={`${user.firstName} ${user.lastName}`} />
        <textarea
          rows={2} maxLength={5000} value={text} onChange={(e) => setText(e.target.value)}
          placeholder={`What's on your mind, ${user.firstName}?`}
        />
      </div>
      {preview && (
        <div className="preview">
          <img src={preview} alt="Selected" />
          <button type="button" className="preview-remove" onClick={() => setFile(null)}>✕</button>
        </div>
      )}
      {error && <div className="error">{error}</div>}
      <div className="composer-actions">
        <button type="button" className="btn-outline" onClick={() => fileInput.current.click()}>Photo</button>
        <input ref={fileInput} type="file" hidden accept="image/jpeg,image/png,image/webp"
               onChange={(e) => { setFile(e.target.files[0] ?? null); e.target.value = ''; }} />
        <button className="btn-primary" disabled={busy || (!text.trim() && !file)}>{busy ? 'Posting…' : 'Post'}</button>
      </div>
    </form>
  );
}
