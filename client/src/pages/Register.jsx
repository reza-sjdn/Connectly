import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

export default function Register() {
  const { register } = useAuth();
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '' });
  const [errors, setErrors] = useState([]);
  const [busy, setBusy] = useState(false);

  const onChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  async function onSubmit(e) {
    e.preventDefault();
    setBusy(true);
    setErrors([]);
    try { await register(form); }
    catch (err) {
      const list = err.errors ? Object.values(err.errors).flat() : [err.message];
      setErrors(list);
    } finally { setBusy(false); }
  }

  return (
    <div className="auth-shell">
      <form className="card" onSubmit={onSubmit}>
        <h2>Create a new account</h2>
        <div className="row">
          <input name="firstName" placeholder="First name" value={form.firstName} onChange={onChange} required />
          <input name="lastName" placeholder="Last name" value={form.lastName} onChange={onChange} required />
        </div>
        <input name="email" type="email" placeholder="Email" value={form.email} onChange={onChange} required />
        <input name="password" type="password" placeholder="New password (min 8 chars)" value={form.password} onChange={onChange} required />
        {errors.length > 0 && <ul className="error">{errors.map((m, i) => <li key={i}>{m}</li>)}</ul>}
        <button className="primary" disabled={busy}>{busy ? 'Creating…' : 'Sign up'}</button>
        <Link className="secondary" to="/login">Already have an account?</Link>
      </form>
    </div>
  );
}
