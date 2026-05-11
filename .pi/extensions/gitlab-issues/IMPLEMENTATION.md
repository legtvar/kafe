# GitLab Issues Extension - Implementation Summary

## Overview

This extension provides GitLab issue integration for the KAFE project, allowing Pi to read and interact with issues from the GitLab repository at `gitlab.fi.muni.cz:legtvar/kafe.git`.

## Files Created

1. **`.pi/extensions/gitlab-issues/index.ts`** - Main extension implementation
2. **`.pi/extensions/gitlab-issues/README.md`** - User documentation
3. **`.pi/config.json`** - Project configuration (optional)
4. **`.pi/skills/gitlab-issues.md`** - Skill documentation for Pi
5. **`.pi/extensions/gitlab-issues/IMPLEMENTATION.md`** - This file

## Key Features Implemented

### 1. Auto-detection of GitLab Repository
- Parses git remote URLs to detect GitLab instances
- Supports both SSH (`git@...`) and HTTPS URLs
- Works with any GitLab instance (gitlab.com, self-hosted, etc.)

### 2. Issue Fetching
- Uses GitLab API v4 to fetch issues
- Supports filtering by state (opened/closed/all)
- Supports search by title/description
- Configurable limits (default: 100 issues)

### 3. Custom Tools

#### `gitlab_list_issues`
Lists issues with optional filtering:
```typescript
{
  name: "gitlab_list_issues",
  parameters: {
    state?: "opened" | "closed" | "all",
    search?: string,
    limit?: number
  }
}
```

#### `gitlab_get_issue`
Retrieves a specific issue by ID:
```typescript
{
  name: "gitlab_get_issue",
  parameters: {
    issue_id: number
  }
}
```

### 4. Autocomplete Support
- Triggers on `!` prefix (GitLab merge request/issue notation)
- Supports numeric search (e.g., `!12` matches issues 12, 120, 1234, etc.)
- Supports fuzzy text search in titles and descriptions
- Returns up to 20 suggestions

### 5. Authentication Support
Checks for authentication tokens in environment variables:
- `GITLAB_CI_TOKEN`
- `GL_TOKEN`
- `GITLAB_TOKEN`

Uses GitLab's `PRIVATE-TOKEN` header for authenticated requests.

## Technical Details

### URL Parsing
The extension handles various GitLab URL formats:
- `git@gitlab.fi.muni.cz:legtvar/kafe.git` → `https://gitlab.fi.muni.cz/api/v4/projects/legtvar%2Fkafe/...`
- `https://gitlab.fi.muni.cz/legtvar/kafe.git` → Same as above

### Caching
Issues are cached after first fetch to avoid repeated API calls:
- Cache persists for the session
- Background loading on session start
- Error handling with user notifications

### Error Handling
- Graceful degradation if not a GitLab repo
- User notifications on fetch failures
- Fallback to default autocomplete if issue loading fails

## Usage Examples

### List all open issues
```
Use gitlab_list_issues() to get all open issues
```

### Search for specific issues
```
Use gitlab_list_issues({ search: "authentication bug", state: "opened" })
```

### Get issue details
```
Use gitlab_get_issue({ issue_id: 123 }) to read issue #123
```

### Use autocomplete
Type `!` followed by numbers or text in any input field to see issue suggestions.

## Testing

To test the extension:
1. Run `pi` in this directory
2. Look for notification: "GitLab Issues extension loaded for legtvar/kafe"
3. Try using the tools or typing `!` for autocomplete

## Future Enhancements

Potential improvements:
- [ ] Support for issue comments/discussions
- [ ] Create/edit issues via tools
- [ ] Support for merge requests
- [ ] Webhook integration for real-time updates
- [ ] Issue templates support
- [ ] Milestone and assignee filtering
- [ ] Local caching with file persistence

## Dependencies

- No external npm dependencies required
- Uses built-in pi types and utilities
- Requires `curl` for API calls (standard in most environments)

## Compatibility

- Works with Pi coding agent
- Compatible with TypeScript via jiti transpiler
- Cross-platform (Linux, macOS, Windows with WSL)
