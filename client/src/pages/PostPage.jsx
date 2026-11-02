import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import PostCard from '../components/PostCard.jsx';
import { useHubEvent } from '../realtime/RealtimeContext.jsx';

export default function PostPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [post, setPost] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    setPost(null);
    api(`/api/posts/${id}`).then(setPost).catch((e) => setError(e.status === 404 ? 'This post is no longer available.' : e.message));
  }, [id]);

  useHubEvent('PostStatsChanged', (postId, likesCount, commentsCount) => {
    if (postId === id) setPost((p) => p && { ...p, likesCount, commentsCount });
  });

  if (error) return <div className="card muted">{error}</div>;
  if (!post) return <div className="center muted">Loading…</div>;

  return (
    <PostCard
      post={post}
      defaultShowComments
      onDeleted={() => navigate('/')}
      onChanged={(_, patch) => setPost((p) => ({ ...p, ...patch }))}
    />
  );
}
