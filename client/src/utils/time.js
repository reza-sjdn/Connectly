// The API sends UTC timestamps; add a "Z" if a proxy/serializer dropped it so browsers don't read them as local time.
export function timeAgo(iso) {
  const hasZone = /[zZ]$|[+-]\d{2}:?\d{2}$/.test(iso);
  const then = new Date(hasZone ? iso : `${iso}Z`).getTime();
  const s = Math.max(0, Math.floor((Date.now() - then) / 1000));

  if (s < 60) return 'just now';
  const m = Math.floor(s / 60);
  if (m < 60) return `${m}m`;
  const h = Math.floor(m / 60);
  if (h < 24) return `${h}h`;
  const d = Math.floor(h / 24);
  if (d < 7) return `${d}d`;
  return new Date(then).toLocaleDateString();
}
