import { useState } from 'react';
import { api } from '../api/client.js';

export default function EditProfileForm({ initial, onSaved, onCancel }) {
  const [form, setForm] = useState({
    firstName: initial.firstName,
    lastName: initial.lastName,
    bio: initial.bio ?? '',
    dateOfBirth: initial.dateOfBirth ?? '',
  });
  const [errors, setErrors] = useState([]);
  const [busy, setBusy] = useState(false);

  const onChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  async function onSubmit(e) {
    e.preventDefault();
    setBusy(true);
    setErrors([]);
    try {
      await api('/api/profiles/me', {
        method: 'PUT',
        body: { ...form, bio: form.bio || null, dateOfBirth: form.dateOfBirth || null },
      });
      await onSaved();
    } catch (err) {
      setErrors(err.errors ? Object.values(err.errors).flat() : [err.message]);
    } finally { setBusy(false); }
  }

  return (
    <form className="card edit-form" onSubmit={onSubmit}>
      <div className="row">
        <input name="firstName" placeholder="First name" value={form.firstName} onChange={onChange} required />
        <input name="lastName" placeholder="Last name" value={form.lastName} onChange={onChange} required />
      </div>
      <textarea name="bio" placeholder="Bio (max 500 characters)" maxLength={500} rows={3} value={form.bio} onChange={onChange} />
      <label className="muted">Date of birth
        <input name="dateOfBirth" type="date" value={form.dateOfBirth} onChange={onChange} />
      </label>
      {errors.length > 0 && <ul className="error">{errors.map((m, i) => <li key={i}>{m}</li>)}</ul>}
      <div className="row">
        <button className="btn-primary" disabled={busy}>{busy ? 'Saving…' : 'Save'}</button>
        <button type="button" className="btn-outline" onClick={onCancel}>Cancel</button>
      </div>
    </form>
  );
}
