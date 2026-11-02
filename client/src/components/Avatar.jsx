import { mediaUrl } from '../api/client.js';

export default function Avatar({ url, name = '', size = 40, online = false }) {
  const src = mediaUrl(url);
  const initials = name.split(' ').filter(Boolean).map((s) => s[0]).join('').slice(0, 2).toUpperCase();
  const style = { width: size, height: size, fontSize: size * 0.4 };

  const face = src
    ? <img className="avatar" src={src} alt={name} style={style} />
    : <span className="avatar avatar-fallback" style={style}>{initials}</span>;

  return (
    <span className="avatar-box">
      {face}
      {online && <span className="online-dot" style={{ width: Math.max(10, size * 0.25), height: Math.max(10, size * 0.25) }} title="Online" />}
    </span>
  );
}
