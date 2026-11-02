import { useState } from 'react';
import { api } from '../api/client.js';

export default function FollowButton({ userId, following, onChange }) {
  const [busy, setBusy] = useState(false);

  async function toggle() {
    setBusy(true);
    try {
      await api(`/api/users/${userId}/follow`, { method: following ? 'DELETE' : 'POST' });
      onChange(!following);
    } catch { /* surfaced by the next reload; keep UI simple */ }
    finally { setBusy(false); }
  }

  return (
    <button className={following ? 'btn-outline' : 'btn-primary'} onClick={toggle} disabled={busy}>
      {following ? 'Following' : 'Follow'}
    </button>
  );
}
