export function describeNotification(n) {
  const who = `${n.actor.firstName} ${n.actor.lastName}`;
  switch (n.type) {
    case 'NewFollower': return { who, text: 'started following you', to: `/profile/${n.actor.id}` };
    case 'PostLiked': return { who, text: 'liked your post', to: `/posts/${n.postId}` };
    case 'PostCommented': return { who, text: 'commented on your post', to: `/posts/${n.postId}` };
    case 'GroupJoinRequested': return { who, text: `asked to join ${n.groupName ?? 'your group'}`, to: `/groups/${n.groupId}` };
    case 'GroupRequestApproved': return { who, text: `approved your request to join ${n.groupName ?? 'the group'}`, to: `/groups/${n.groupId}` };
    default: return { who, text: 'sent you a notification', to: '/' };
  }
}
