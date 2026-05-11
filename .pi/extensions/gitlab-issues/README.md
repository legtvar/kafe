# GitLab Issues Extension

A Pi extension that integrates GitLab issues into your coding workflow for the KAFE project.

## Features

- **List Issues**: Use the `gitlab_list_issues` tool to list all open/closed issues with filtering and search
- **Get Issue Details**: Use the `gitlab_get_issue` tool to retrieve full details of a specific issue
- **Autocomplete**: Type `!` followed by numbers or text to get autocomplete suggestions for issues
- **Auto-detection**: Automatically detects GitLab repositories (supports gitlab.fi.muni.cz and other instances)

## Usage

### Tools

#### List Issues

```
gitlab_list_issues(state?: "opened" | "closed" | "all", search?: string, limit?: number)
```

Examples:
- List all open issues: `gitlab_list_issues()`
- List closed issues: `gitlab_list_issues({ state: "closed" })`
- Search for issues: `gitlab_list_issues({ search: "bug" })`
- Limit results: `gitlab_list_issues({ limit: 10 })`

#### Get Specific Issue

```
gitlab_get_issue(issue_id: number)
```

Example:
- Get issue #123: `gitlab_get_issue({ issue_id: 123 })`

### Autocomplete

Type `!` followed by issue number or partial title to get autocomplete suggestions:
- `!12` → Shows issues starting with 12
- `!bug` → Shows issues matching "bug" in title/description

## Authentication

The extension supports authentication via environment variables:
- `GITLAB_CI_TOKEN` - GitLab CI token
- `GL_TOKEN` - Generic GitLab token
- `GITLAB_TOKEN` - Alternative token variable

For private repositories, set one of these tokens before running pi.

## Installation

This extension is already installed in `.pi/extensions/gitlab-issues/`. It will be automatically loaded when you run pi in this project directory.

To reload the extension after changes, use `/reload` command in pi.

## Requirements

- Git repository with a GitLab remote
- GitLab API access (public or authenticated for private repos)
- `curl` command available in PATH

## Supported GitLab Instances

- GitLab.com
- Self-hosted GitLab (e.g., gitlab.fi.muni.cz)
- Any GitLab instance accessible via HTTP/HTTPS

## Example Workflow

1. Ask pi to check what issues are currently open
2. Use autocomplete to reference issues in comments (`!123`)
3. Have pi read specific issues to understand requirements
4. Filter issues by search terms to find relevant work items
