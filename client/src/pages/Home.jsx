import { Link } from 'react-router-dom';
import Feed from '../components/Feed.jsx';
import StoryBar from '../components/StoryBar.jsx';

export default function Home() {
  return (
    <>
      <StoryBar />
      <Feed
        url="/api/posts/feed"
        composer
        emptyText={<>Your feed is empty. Write your first post, <Link to="/people">follow people</Link> or <Link to="/groups">join a group</Link>.</>}
      />
    </>
  );
}
