# GitLab Issues Extension - Quick Start

## What This Does

This extension lets Pi read GitLab issues from the KAFE project repository. You can now ask Pi about issues, search for them, and get details.

## Quick Examples

### 1. List Open Issues
Just ask:
```
"Can you list all open issues?"
```
Pi will use `gitlab_list_issues()` to fetch and display them.

### 2. Search for Specific Issues
```
"Find issues related to authentication"
```
Pi will search using: `gitlab_list_issues({ search: "authentication" })`

### 3. Get Issue Details
```
"Read issue #123"
```
Pi will use: `gitlab_get_issue({ issue_id: 123 })`

### 4. Use Autocomplete
When typing in Pi, type `!` followed by numbers or text:
- `!12` → Shows issues starting with 12
- `!bug` → Shows issues matching "bug"

## Common Workflows

### Understanding Current Work
```
"What are the current open issues we're working on?"
→ Lists all open issues
```

### Finding Related Issues
```
"Find issues about API improvements"
→ Searches for "API improvements"
```

### Getting Context Before Starting Work
```
"Read issue !456 so I understand what needs to be done"
→ Fetches full details of issue #456
```

### Checking Closed Issues
```
"What issues were closed recently?"
→ Lists closed issues
```

## Tips

- **Be specific**: When searching, use specific keywords
- **Use autocomplete**: Type `!` to quickly reference issues
- **Combine filters**: You can search AND filter by state
- **Rate limits**: If you make many requests, wait a moment between them

## Troubleshooting

### "Extension not loaded"
- Make sure you're in the KAFE project directory
- Run `/reload` in Pi to reload extensions
- Check that `.pi/extensions/gitlab-issues/index.ts` exists

### "Failed to load issues"
- Check your internet connection
- For private repos, set `GITLAB_TOKEN` environment variable
- The repository must be accessible via GitLab API

### "No issues found"
- Try without filters first
- Check if issues exist in the repository
- Verify the GitLab remote is configured correctly

## Need Help?

See the full documentation:
- User guide: `README.md`
- Technical details: `IMPLEMENTATION.md`
- Skills doc: `.pi/skills/gitlab-issues.md`
