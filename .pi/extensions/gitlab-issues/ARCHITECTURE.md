# Architecture Overview

## Extension Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     Pi Session Start                        │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              gitlab-issues Extension Loaded                 │
│  • Detects GitLab repository from git remote                │
│  • Parses URL: git@gitlab.fi.muni.cz:legtvar/kafe.git      │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  Background Issue Loading                   │
│  • Fetches issues via GitLab API v4                         │
│  • URL: https://gitlab.fi.muni.cz/api/v4/projects/...      │
│  • Caches results for session                               │
│  • Shows notification on success/error                      │
└──────────────────────────┬──────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│   Tool:      │  │   Tool:      │  │ Autocomplete │
│ list_issues  │  │ get_issue    │  │   Provider   │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                  │
       │                 │                  │
       ▼                 ▼                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    User Interaction                         │
│  • LLM calls tools to fetch issue data                      │
│  • User types ! for autocomplete suggestions                │
│  • Issues displayed in formatted output                     │
└─────────────────────────────────────────────────────────────┘
```

## Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    index.ts (Extension)                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────┐     ┌─────────────────┐               │
│  │ detectGitLabRepo│     │ parseGitLabRemote│              │
│  └────────┬────────┘     └────────┬────────┘               │
│           │                       │                         │
│           └──────────┬────────────┘                         │
│                      ▼                                       │
│            ┌──────────────────┐                             │
│            │   RepoInfo       │                             │
│            │  - projectId     │                             │
│            │  - gitlabUrl     │                             │
│            │  - remoteUrl     │                             │
│            └────────┬─────────┘                             │
│                     │                                       │
│  ┌──────────────────▼──────────────────┐                   │
│  │      fetchGitLabIssues()            │                   │
│  │  • Constructs API URL               │                   │
│  │  • Adds auth headers if available   │                   │
│  │  • Calls curl to fetch data         │                   │
│  │  • Parses JSON response             │                   │
│  └──────────────────┬──────────────────┘                   │
│                     │                                       │
│  ┌──────────────────▼──────────────────┐                   │
│  │         Issue Cache                 │                   │
│  │  • In-memory cache per session      │                   │
│  │  • Lazy loading on first access     │                   │
│  │  • Error handling with fallback     │                   │
│  └──────────────────┬──────────────────┘                   │
│                     │                                       │
│  ┌──────────────────▼──────────────────┐                   │
│  │      Tool Registrations             │                   │
│  │  • gitlab_list_issues               │                   │
│  │  • gitlab_get_issue                 │                   │
│  └──────────────────┬──────────────────┘                   │
│                     │                                       │
│  ┌──────────────────▼──────────────────┐                   │
│  │   Autocomplete Provider             │                   │
│  │  • Triggers on ! prefix             │                   │
│  │  • Fuzzy matching on title/desc     │                   │
│  │  • Numeric prefix matching          │                   │
│  └─────────────────────────────────────┘                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

### Tool Call: `gitlab_list_issues`

```
User/LLM Request
       │
       ▼
┌────────────────────────┐
│ Tool Parameters        │
│ • state: "opened"      │
│ • search: "bug"        │
│ • limit: 10            │
└──────────┬─────────────┘
           │
           ▼
┌────────────────────────┐
│ Check Cache            │
│ • If cached, use it    │
│ • If not, fetch        │
└──────────┬─────────────┘
           │
           ▼
┌────────────────────────┐
│ Apply Filters          │
│ • Filter by state      │
│ • Filter by search     │
│ • Apply limit          │
└──────────┬─────────────┘
           │
           ▼
┌────────────────────────┐
│ Format Output          │
│ • Markdown formatting  │
│ • Include metadata     │
│ • Add URLs             │
└──────────┬─────────────┘
           │
           ▼
┌────────────────────────┐
│ Return to LLM/User     │
└────────────────────────┘
```

### Autocomplete Flow

```
User Types: "!12"
      │
      ▼
┌──────────────────────┐
│ Extract Token        │
│ • Prefix: !          │
│ • Query: 12          │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Get Cached Issues    │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Filter Issues        │
│ • Numeric match:     │
│   Starts with "12"   │
│ • OR fuzzy match:    │
│   "12" in title      │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Format Suggestions   │
│ • Label: !123        │
│ • Description:       │
│   [OPEN] Title       │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Display to User      │
│ • Up to 20 items     │
└──────────────────────┘
```

## API Endpoints Used

### GitLab API v4

```
GET /api/v4/projects/:id/issues

Parameters:
• state: opened|closed|all
• per_page: number (max 100)
• search: string (optional)

Authentication:
• Header: PRIVATE-TOKEN: <token>
• Or via query parameter

Response: Array of Issue objects
```

## Error Handling Strategy

```
┌─────────────────┐
│  GitLab API     │
│  Call Fails     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Log Error      │
│  (internal)     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Show User      │
│  Notification   │
│  (once per sess)│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Return Empty   │
│  or Cached      │
│  Data           │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Continue       │
│  Operation      │
│  (graceful)     │
└─────────────────┘
```

## Security Considerations

1. **Token Storage**: Tokens from environment variables only, never stored
2. **URL Validation**: Remote URLs validated before API calls
3. **Rate Limiting**: Built-in limits (100 issues max per request)
4. **No Write Access**: Read-only operations, no issue creation/modification
5. **CORS**: Uses curl, bypasses browser CORS restrictions

## Performance Optimizations

1. **Lazy Loading**: Issues loaded in background on session start
2. **In-Memory Cache**: Single fetch per session
3. **Client-Side Filtering**: No re-fetching for different filters
4. **Fuzzy Matching**: Fast local filtering instead of API calls
5. **Timeout Protection**: 10-second timeout on API calls

## Extensibility Points

The architecture supports future enhancements:

1. **Write Operations**: Add tools for creating/editing issues
2. **Comments**: Fetch and display issue comments
3. **Merge Requests**: Similar implementation for MRs
4. **Webhooks**: Real-time updates via webhook listeners
5. **Persistent Cache**: File-based cache across sessions
6. **Offline Mode**: Work with cached data when offline
