import { useEffect, useRef, useState } from 'react';
import { api } from '../api/client.js';

const COLORS = ['#1877f2', '#e41e3f', '#31a24c', '#8e44ad', '#f39c12', '#242526'];

export default function StoryCreator({ onClose, onCreated }) {
  const [mode, setMode] = useState('text');   // 'text' | 'photo'
  const [text, setText] = useState('');
  const [color, setColor] = useState(COLORS[0]);
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState(null);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const fileInput = useRef(null);

  useEffect(() => {
    if (!file) { setPreview(null); return undefined; }
    const url = URL.createObjectURL(file);
    setPreview(url);
    return () => URL.revokeObjectURL(url);
  }, [file]);

  const ready = mode === 'text' ? text.trim().length > 0 : !!file;

  async function submit() {
    setBusy(true);
    setError('');
    try {
      const body = new FormData();
      if (mode === 'text') { body.append('text', text.trim()); body.append('backgroundColor', color); }
      else body.append('image', file);
      await api('/api/stories', { method: 'POST', body });
      onCreated();
    } catch (e) {
      setError(e.errors ? Object.values(e.errors).flat().join(' ') : e.message);
    } finally { setBusy(false); }
  }

  return (
    <div className="overlay" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="row between">
          <h3>Create a story</h3>
          <button className="link" onClick={onClose}>Close</button>
        </div>

        <div className="tabs">
          <button className={mode === 'text' ? 'active' : ''} onClick={() => setMode('text')}>Text</button>
          <button className={mode === 'photo' ? 'active' : ''} onClick={() => setMode('photo')}>Photo</button>
        </div>

        {mode === 'text' ? (
          <>
            <div className="story-preview" style={{ background: color }}>
              <textarea maxLength={280} placeholder="Start typing…" value={text} onChange={(e) => setText(e.target.value)} />
            </div>
            <div className="swatches">
              {COLORS.map((c) => (
                <button key={c} type="button" className={`swatch ${c === color ? 'on' : ''}`} style={{ background: c }}
                        onClick={() => setColor(c)} aria-label={`Background ${c}`} />
              ))}
            </div>
          </>
        ) : (
          <>
            <div className="story-preview photo">
              {preview ? <img src={preview} alt="Story preview" /> : <span className="muted">No photo selected</span>}
            </div>
            <button className="btn-outline" onClick={() => fileInput.current.click()}>Choose photo</button>
            <input ref={fileInput} type="file" hidden accept="image/jpeg,image/png,image/webp"
                   onChange={(e) => { setFile(e.target.files[0] ?? null); e.target.value = ''; }} />
          </>
        )}

        {error && <div className="error">{error}</div>}
        <p className="muted small">Your story disappears after 24 hours and is shown to people who follow you.</p>
        <button className="btn-primary" disabled={!ready || busy} onClick={submit}>{busy ? 'Sharing…' : 'Share story'}</button>
      </div>
    </div>
  );
}
