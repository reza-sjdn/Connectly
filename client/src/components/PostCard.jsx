import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api, mediaUrl } from '../api/client.js';
import { timeAgo } from '../utils/time.js';
import Avatar from './Avatar.jsx';
import CommentSection from './CommentSection.jsx';

export default function PostCard({ post, onDeleted, onChanged, defaultShowComments = false }) {
  const [showComments, setShowComments] = useState(defaultShowComments);
  const [busy, setBusy] = useState(false);
  const name = `${post.author.firstName} ${post.author.lastName}`;

  async function toggleLike() {
    if (busy) return;
    setBusy(true);
    try {
      const state = await api(`/api/posts/${post.id}/like`, { method: post.isLikedByMe ? 'DELETE' : 'POST' });
      onChanged(post.id, { isLikedByMe: state.isLiked, likesCount: state.likesCount });
    } finally { setBusy(false); }
  }

  async function remove() {
    if (!window.confirm('Delete this post?')) return;
    await api(`/api/posts/${post.id}`, { method: 'DELETE' });
    onDeleted(post.id);
  }

  return (
    <article className="card post">
      <header className="post-head">
        <Link to={`/profile/${post.author.id}`} className="user-link">
          <Avatar url={post.author.avatarUrl} name={name} />
          <span>
            <span className="author-name">{name}</span>
            <span className="muted small block">{timeAgo(post.createdAtUtc)}</span>
          </span>
        </Link>
        {post.isMine && <button className="link small" onClick={remove}>Delete</button>}
      </header>

      {post.groupId && (
        <div className="muted small">Posted in <Link to={`/groups/${post.groupId}`}>{post.groupName}</Link></div>
      )}

      {post.content && <p className="pre post-text">{post.content}</p>}
      {post.imageUrl && <img className="post-image" src={mediaUrl(post.imageUrl)} alt="" loading="lazy" />}

      <div className="post-stats muted small">
        <span>{post.likesCount} {post.likesCount === 1 ? 'like' : 'likes'}</span>
        <span>{post.commentsCount} {post.commentsCount === 1 ? 'comment' : 'comments'}</span>
      </div>

      <div className="post-actions">
        <button className={post.isLikedByMe ? 'liked' : ''} onClick={toggleLike} disabled={busy}>
          {post.isLikedByMe ? '👍 Liked' : '👍 Like'}
        </button>
        <button onClick={() => setShowComments((v) => !v)}>💬 Comment</button>
      </div>

      {showComments && (
        <CommentSection
          postId={post.id}
          onCountChange={(delta) => onChanged(post.id, { commentsCount: Math.max(0, post.commentsCount + delta) })}
        />
      )}
    </article>
  );
}
