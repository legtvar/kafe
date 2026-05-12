---
name: gitlab-issues
description: GitLab issues integration for KAFE project - list, search, and retrieve issues from GitLab
---

# GitLab Issues Integration

This project has a custom Pi extension that integrates with GitLab issues.

## Available Tools

### `gitlab_list_issues`
Lists GitLab issues for the KAFE project.

**Parameters:**
- `state` (optional): "opened", "closed", or "all" - defaults to "opened"
- `search` (optional): Text to search in issue titles and descriptions
- `limit` (optional): Maximum number of issues to return (1-100)

**Example usage:**
```
- List all open issues
- Find closed issues related to "authentication"
- Get the 5 most recent issues
```

### `gitlab_get_issue`
Retrieves full details of a specific GitLab issue.

**Parameters:**
- `issue_id` (required): The issue ID (IID) to retrieve

**Example usage:**
```
- Get details of issue #123
- Read the full description and comments of an issue
```

## Autocomplete Support

Type `!` followed by numbers or text to autocomplete issue references:
- `!12` → Matches issues starting with 12
- `!bug` → Matches issues containing "bug"

## Tips

1. **Issue References**: When discussing work, reference issues using `!123` format
2. **Search**: Use the search parameter to find issues by keywords
3. **State Filtering**: Filter by state to focus on active work vs completed items
4. **Rate Limits**: If you hit rate limits, wait a moment before making more requests

## Authentication

For private repositories or to increase rate limits, set one of these environment variables:
- `GITLAB_CI_TOKEN`
- `GL_TOKEN`
- `GITLAB_TOKEN`
