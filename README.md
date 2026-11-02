# Connectly – Facebook-style social network

ASP.NET Core 10 · EF Core · SQL Server · Clean Architecture · CQRS (MediatR) · SignalR · React (Vite)

## Layout
```
src/
  Connectly.Domain          entities, no dependencies
  Connectly.Application     CQRS commands/queries, validators, abstractions (IApplicationDbContext, ITokenService, IFileStorage…)
  Connectly.Infrastructure  EF Core + Identity, JWT/refresh tokens, local file storage
  Connectly.Api             controllers, auth wiring, exception handling
client/                     React (Vite, plain JS, Context API)
```
Dependency rule: Domain ← Application ← Infrastructure ← Api.

## Run (backend)
```bash
cd src/Connectly.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "<at least 32 random characters>"

# create the first migration (once)
dotnet ef migrations add Initial -p ../Connectly.Infrastructure -s . -o Persistence/Migrations

dotnet run          # http://localhost:5080 (applies migrations automatically in Development)
```
OpenAPI document: http://localhost:5080/openapi/v1.json

## Run (frontend)
```bash
cd client
npm install
npm run dev         # http://localhost:5173
```

## Auth flow
- `POST /api/auth/register | login` → `{ accessToken, refreshToken, accessTokenExpiresAtUtc }`
- Access token: JWT, 15 min. Refresh token: random, stored hashed (SHA-256), rotated on every use, reuse of a rotated token revokes all of that user's sessions.
- `POST /api/auth/refresh`, `POST /api/auth/logout`, `GET /api/profiles/me` (authorized)

## Slice 2 additions
- `PUT /api/profiles/me`, `POST /api/profiles/me/avatar|cover` (multipart field `file`, JPEG/PNG/WebP, 5 MB, magic-byte checked)
- `GET /api/profiles/{id}`, `GET /api/profiles/suggestions`
- Follow (Instagram-style, one-way): `POST|DELETE /api/users/{id}/follow`, `GET /api/users/{id}/followers|following?page=&pageSize=`
- After pulling slice 2 run: `dotnet ef migrations add Follows -p ../Connectly.Infrastructure -s . -o Persistence/Migrations`

## Slice 3 additions
- Posts (text and/or one image): `POST /api/posts` (multipart: `content`, `image`), `GET /api/posts/feed?cursor=&pageSize=`, `GET /api/users/{id}/posts`, `GET|DELETE /api/posts/{id}`
- Feed = my posts + posts of people I follow, newest first, keyset (cursor) paging
- Likes (like-only): `POST|DELETE /api/posts/{id}/like` → `{ isLiked, likesCount }`
- Comments: `GET|POST /api/posts/{id}/comments`, `DELETE /api/comments/{id}` (comment author or post owner)
- All DateTimes are now read back as UTC (value converter) so the API serializes them with a `Z`
- Migration: `dotnet ef migrations add Posts -p ../Connectly.Infrastructure -s . -o Persistence/Migrations`

## Slice 4 additions (real-time)
- SignalR hub at `/hubs/app` (JWT via `?access_token=`). Server → client: `NotificationReceived`, `MessageReceived`, `MessagesRead`, `CommentAdded`, `PostStatsChanged`, `PresenceChanged`, `Typing`. Client → server: `Typing(otherUserId, isTyping)`
- Notifications (stored): new follower, post liked, post commented. `GET /api/notifications`, `GET /api/notifications/unread-count`, `POST /api/notifications/{id}/read`, `POST /api/notifications/read-all`
- 1:1 chat: `GET /api/chat/conversations`, `GET|POST /api/chat/with/{userId}/messages`, `POST /api/chat/with/{userId}/read`, `GET /api/chat/unread-count`
- Presence: `POST /api/presence/online` (only reveals users you follow / are followed by / chat with). In-memory tracker = single server; scaling out needs Redis + SignalR backplane.
- Run: `npm install` in `client/` (adds @microsoft/signalr), then `dotnet ef migrations add Realtime -p ../Connectly.Infrastructure -s . -o Persistence/Migrations`

## Slice 5 additions (groups, stories, search)
- **Groups** (public / private-with-approval): `GET /api/groups?scope=Mine|Discover`, `POST /api/groups`, `GET|PUT /api/groups/{id}`, `POST /api/groups/{id}/join`, `DELETE /api/groups/{id}/membership`, members (`GET`, `PUT .../members/{userId}/role`, `DELETE .../members/{userId}`), join requests (`GET /requests`, `POST /requests/{userId}/approve`, `DELETE /requests/{userId}`), `GET /api/groups/{id}/posts`, group chat (`GET|POST /api/groups/{id}/messages`, live event `GroupMessageReceived`)
- Group posts: `POST /api/posts` accepts a `groupId` form field. Group posts show in the feed of active members only and never on public timelines. Every single-post read/like/comment goes through a visibility filter (private group = members only).
- Notifications: `GroupJoinRequested` (to admins), `GroupRequestApproved` (to the requester)
- **Stories**: photo OR text on a colored background, 24h. `POST /api/stories`, `GET /api/stories` (me + people I follow, grouped per author), `DELETE /api/stories/{id}`, `POST /api/stories/{id}/view`, `GET /api/stories/{id}/viewers` (author only). A background job removes expired rows and images every 30 minutes.
- **Search** (plain LIKE, wildcard-escaped): `GET /api/search?q=` -> people, groups, posts
- Migration: `dotnet ef migrations add GroupsStoriesSearch -p ../Connectly.Infrastructure -s . -o Persistence/Migrations`

## Roadmap (vertical slices)
1. ✅ Foundation + auth + profile shell
2. ✅ Profile editing, avatar/cover upload, follow system
3. ✅ Posts, image upload, comments, likes, news feed
4. ✅ SignalR: notifications, 1:1 chat, presence + typing (group chat comes with Groups)
5. Groups, stories (24h expiry), search
