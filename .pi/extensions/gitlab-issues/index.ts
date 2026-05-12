/**
 * GitLab Issues Extension for KAFE Project
 *
 * Provides tools to read and search GitLab issues from the current repository.
 * Supports both GitLab.com and self-hosted GitLab instances.
 *
 * Usage:
 * - Use the `gitlab_list_issues` tool to list issues
 * - Use the `gitlab_get_issue` tool to get a specific issue
 * - Type # followed by partial issue number or title for autocomplete
 */

import type { ExtensionAPI, ExtensionContext } from "@earendil-works/pi-coding-agent";
import { Type } from "@earendil-works/pi-ai";
import {
	type AutocompleteItem,
	type AutocompleteProvider,
	type AutocompleteSuggestions,
	fuzzyFilter,
} from "@earendil-works/pi-tui";

type GitLabIssue = {
	id: number;
	iid: number;
	project_id: number;
	title: string;
	description: string;
	state: "opened" | "closed" | "all";
	created_at: string;
	updated_at: string;
	author: {
		id: number;
		name: string;
		username: string;
	};
	labels?: string[];
	web_url: string;
};

type RepoInfo = {
	projectId: number;
	gitlabUrl: string;
	remoteUrl: string;
};

const MAX_ISSUES = 100;
const MAX_SUGGESTIONS = 20;

/**
 * Parse GitLab remote URL to extract project path and instance URL
 */
function parseGitLabRemote(remoteUrl: string): RepoInfo | null {
	let gitlabUrl: string;
	let projectPath: string;

	// SSH format: git@gitlab.example.com:group/project.git
	const sshMatch = remoteUrl.match(/^git@([^:]+):([^/]+\/.+?)(?:\.git)?$/);
	if (sshMatch) {
		gitlabUrl = `https://${sshMatch[1]}`;
		projectPath = sshMatch[2];
	} else {
		// HTTPS format: https://gitlab.example.com/group/project.git
		const httpsMatch = remoteUrl.match(/^https?:\/\/([^/]+)\/(.+?)(?:\.git)?$/);
		if (httpsMatch) {
			gitlabUrl = `${httpsMatch[1].startsWith("http") ? "" : "https:"}//${httpsMatch[1]}`;
			projectPath = httpsMatch[2];
		} else {
			return null;
		}
	}

	// Extract numeric project ID from the URL if available
	const projectIdMatch = projectPath.match(/\/(\d+)$/);
	const projectId = projectIdMatch ? parseInt(projectIdMatch[1], 10) : -1;

	return {
		projectId,
		gitlabUrl,
		remoteUrl: projectPath,
	};
}

/**
 * Check if the current directory is a GitLab repository
 */
async function detectGitLabRepo(pi: ExtensionAPI, cwd: string): Promise<RepoInfo | null> {
	try {
		const result = await pi.exec("git", ["remote", "-v"], { cwd, timeout: 5_000 });
		if (result.code !== 0) {
			return null;
		}

		for (const line of result.stdout.split("\n")) {
			const columns = line.trim().split(/\s+/);
			const remoteUrl = columns[1];
			if (!remoteUrl) {
				continue;
			}

			// Check if it's a GitLab remote
			if (remoteUrl.includes("gitlab") || remoteUrl.includes("gitlab.fi.muni.cz")) {
				const repoInfo = parseGitLabRemote(remoteUrl);
				if (repoInfo) {
					return repoInfo;
				}
			}
		}

		return null;
	} catch {
		return null;
	}
}

/**
 * Fetch issues from GitLab API
 */
async function fetchGitLabIssues(
	pi: ExtensionAPI,
	repoInfo: RepoInfo,
	options?: { state?: string; search?: string; limit?: number },
): Promise<GitLabIssue[]> {
	const { gitlabUrl, remoteUrl } = repoInfo;
	const encodedProject = encodeURIComponent(remoteUrl);

	const params = new URLSearchParams();
	params.set("state", options?.state || "opened");
	params.set("per_page", String(options?.limit || MAX_ISSUES));

	if (options?.search) {
		params.set("search", options.search);
	}

	const apiUrl = `${gitlabUrl}/api/v4/projects/${encodedProject}/issues?${params.toString()}`;

	// Try to get authentication token from environment or git config
	const authHeaders: Record<string, string> = {};

	// Check for GitLab CI token
	const ciToken = process.env.GITLAB_CI_TOKEN || process.env.GL_TOKEN || process.env.GITLAB_TOKEN;
	if (ciToken) {
		authHeaders["PRIVATE-TOKEN"] = ciToken;
	}

	const result = await pi.exec("curl", ["-s", "-H", "Content-Type: application/json", ...Object.entries(authHeaders).flatMap(([k, v]) => ["-H", `${k}: ${v}`]), apiUrl], {
		cwd: process.cwd(),
		timeout: 10_000,
	});

	if (result.code !== 0) {
		throw new Error(`Failed to fetch issues: ${result.stderr || "Unknown error"}`);
	}

	try {
		return JSON.parse(result.stdout) as GitLabIssue[];
	} catch (error) {
		throw new Error(`Failed to parse issues response: ${error instanceof Error ? error.message : "Unknown error"}`);
	}
}

/**
 * Format an issue for autocomplete display
 */
function formatIssueItem(issue: GitLabIssue): AutocompleteItem {
	const stateLabel = issue.state === "opened" ? "OPEN" : "CLOSED";
	return {
		value: `!${issue.iid}`,
		label: `!${issue.iid}`,
		description: `[${stateLabel}] ${issue.title}`,
	};
}

/**
 * Filter issues based on query
 */
function filterIssues(issues: GitLabIssue[], query: string): AutocompleteItem[] {
	if (!query.trim()) {
		return issues.slice(0, MAX_SUGGESTIONS).map(formatIssueItem);
	}

	// Numeric search for issue numbers
	if (/^\d+$/.test(query)) {
		const numericMatches = issues
			.filter((issue) => String(issue.iid).startsWith(query))
			.slice(0, MAX_SUGGESTIONS)
			.map(formatIssueItem);
		if (numericMatches.length > 0) {
			return numericMatches;
		}
	}

	// Fuzzy search on title and description
	return fuzzyFilter(issues, query, (issue) => `${issue.iid} ${issue.title} ${issue.description}`)
		.slice(0, MAX_SUGGESTIONS)
		.map(formatIssueItem);
}

/**
 * Create autocomplete provider for GitLab issues
 */
function createIssueAutocompleteProvider(
	current: AutocompleteProvider,
	getIssues: () => Promise<GitLabIssue[] | undefined>,
): AutocompleteProvider {
	return {
		async getSuggestions(lines, cursorLine, cursorCol, options): Promise<AutocompleteSuggestions | null> {
			const currentLine = lines[cursorLine] ?? "";
			const textBeforeCursor = currentLine.slice(0, cursorCol);

			// Look for !pattern (GitLab uses ! for issues in some contexts) or #pattern
			const match = textBeforeCursor.match(/(?:^|[ \t])([#!])(\d*[^\s#!]*)$/);
			if (!match) {
				return current.getSuggestions(lines, cursorLine, cursorCol, options);
			}

			const prefix = match[1]; // # or !
			const token = match[2];

			// Only provide suggestions for ! prefix (GitLab merge requests use !)
			// For issues, we can support both but prioritize !
			if (prefix !== "!") {
				return current.getSuggestions(lines, cursorLine, cursorCol, options);
			}

			const issues = await getIssues();
			if (options.signal.aborted || !issues || issues.length === 0) {
				return current.getSuggestions(lines, cursorLine, cursorCol, options);
			}

			const suggestions = filterIssues(issues, token);
			if (suggestions.length === 0) {
				return current.getSuggestions(lines, cursorLine, cursorCol, options);
			}

			return {
				items: suggestions,
				prefix: `!${token}`,
			};
		},

		applyCompletion(lines, cursorLine, cursorCol, item, prefix) {
			return current.applyCompletion(lines, cursorLine, cursorCol, item, prefix);
		},

		shouldTriggerFileCompletion(lines, cursorLine, cursorCol) {
			return current.shouldTriggerFileCompletion?.(lines, cursorLine, cursorCol) ?? true;
		},
	};
}

export default function (pi: ExtensionAPI) {
	pi.on("session_start", async (_event, ctx) => {
		const repoInfo = await detectGitLabRepo(pi, ctx.cwd);
		if (!repoInfo) {
			return; // Not a GitLab repository
		}

		ctx.ui.notify(`GitLab Issues extension loaded for ${repoInfo.remoteUrl}`, "info");

		let issuesCache: GitLabIssue[] | undefined;
		let cachePromise: Promise<GitLabIssue[]> | undefined;
		let loadErrorShown = false;

		const getIssues = async (): Promise<GitLabIssue[] | undefined> => {
			if (issuesCache) {
				return issuesCache;
			}

			cachePromise ||= (async () => {
				try {
					const issues = await fetchGitLabIssues(pi, repoInfo);
					issuesCache = issues;
					return issues;
				} catch (error) {
					if (!loadErrorShown) {
						loadErrorShown = true;
						ctx.ui.notify(
							`GitLab Issues: Failed to load issues - ${error instanceof Error ? error.message : "Unknown error"}`,
							"error",
						);
					}
					return [];
				}
			})();

			return cachePromise;
		};

		// Preload issues in background
		void getIssues();

		// Add autocomplete provider
		ctx.ui.addAutocompleteProvider((current) => createIssueAutocompleteProvider(current, getIssues));

		// Register custom tool to list issues
		pi.registerTool({
			name: "gitlab_list_issues",
			label: "List GitLab Issues",
			description:
				"List GitLab issues for the current project. Supports filtering by state (opened/closed) and searching by text.",
			parameters: Type.Object({
				state: Type.Optional(
					Type.Enum(
						{
							opened: "Opened issues only",
							closed: "Closed issues only",
							all: "All issues",
						},
						{ description: "Filter issues by state" },
					),
				),
				search: Type.Optional(Type.String({ description: "Search term to filter issues by title/description" })),
				limit: Type.Optional(Type.Number({ description: "Maximum number of issues to return", minimum: 1, maximum: 100 })),
			}),
			async execute(_toolCallId, params, _signal, onUpdate, _ctx) {
				onUpdate({ content: [{ type: "text", text: "Fetching issues..." }] });

				const issues = await getIssues();
				if (!issues || issues.length === 0) {
					return {
						content: [{ type: "text", text: "No issues found or failed to load issues." }],
						details: {},
					};
				}

				// Apply filters
				let filtered = issues;
				if (params.state && params.state !== "all") {
					filtered = filtered.filter((issue) => issue.state === params.state);
				}
				if (params.search) {
					const searchLower = params.search.toLowerCase();
					filtered = filtered.filter(
						(issue) =>
							issue.title.toLowerCase().includes(searchLower) ||
							issue.description.toLowerCase().includes(searchLower),
					);
				}
				if (params.limit) {
					filtered = filtered.slice(0, Math.min(params.limit, MAX_ISSUES));
				}

				const output = filtered
					.map(
						(issue) =>
							`#${issue.iid} [${issue.state.toUpperCase()}] ${issue.title}\n` +
							`  Author: ${issue.author.name} (@${issue.author.username})\n` +
							`  Created: ${new Date(issue.created_at).toLocaleDateString()}\n` +
							`  Labels: ${(issue.labels || []).join(", ") || "None"}\n` +
							`  URL: ${issue.web_url}`,
					)
					.join("\n\n");

				return {
					content: [{ type: "text", text: output }],
					details: { count: filtered.length, total: issues.length },
				};
			},
		});

		// Register custom tool to get a specific issue
		pi.registerTool({
			name: "gitlab_get_issue",
			label: "Get GitLab Issue",
			description: "Get details of a specific GitLab issue by its ID.",
			parameters: Type.Object({
				issue_id: Type.Number({ description: "The issue ID (IID) to retrieve" }),
			}),
			async execute(_toolCallId, params, _signal, onUpdate, _ctx) {
				onUpdate({ content: [{ type: "text", text: `Fetching issue #${params.issue_id}...` }] });

				try {
					const allIssues = await getIssues();
					const issue = allIssues?.find((i) => i.iid === params.issue_id);

					if (!issue) {
						return {
							content: [{ type: "text", text: `Issue #${params.issue_id} not found.` }],
							details: {},
						};
					}

					const output =
						`# Issue #${issue.iid}: ${issue.title}\n\n` +
						`**State**: ${issue.state.toUpperCase()}\n` +
						`**Author**: ${issue.author.name} (@${issue.author.username})\n` +
						`**Created**: ${new Date(issue.created_at).toLocaleString()}\n` +
						`**Updated**: ${new Date(issue.updated_at).toLocaleString()}\n` +
						`**Labels**: ${(issue.labels || []).join(", ") || "None"}\n\n` +
						`## Description\n\n${issue.description}\n\n` +
						`---\n` +
						`View on GitLab: ${issue.web_url}`;

					return {
						content: [{ type: "text", text: output }],
						details: {
							issue_id: issue.iid,
							state: issue.state,
							url: issue.web_url,
						},
					};
				} catch (error) {
					return {
						content: [
							{
								type: "text",
								text: `Failed to fetch issue #${params.issue_id}: ${error instanceof Error ? error.message : "Unknown error"}`,
							},
						],
						details: {},
					};
				}
			},
		});
	});
}
