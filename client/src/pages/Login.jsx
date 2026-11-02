import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

export default function Login() {
  const { login } = useAuth();
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  const onChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  async function onSubmit(e) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try { await login(form.email, form.password); }
    catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }

  return (
    <div className="auth-shell">
      <div className="brand">
        <h1>connectly</h1>
        <p>Connect with friends and the world around you.</p>
      </div>
      <form className="card" onSubmit={onSubmit}>
        <input name="email" type="email" placeholder="Email" value={form.email} onChange={onChange} required />
        <input name="password" type="password" placeholder="Password" value={form.password} onChange={onChange} required />
        {error && <div className="error">{error}</div>}
        <button className="primary" disabled={busy}>{busy ? 'Logging in…' : 'Log in'}</button>
        <hr />
        <Link className="secondary" to="/register">Create new account</Link>
      </form>
    </div>
  );
}
