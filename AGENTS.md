# Development Rules

## Conversational Style

- Keep answers short and concise.
- No emojis in commits, issues, PR comments, or code.
- No fluff or cheerful filler text (e.g., "Thanks @user" not "Thanks so much @user!").
- Technical prose only, be direct.
- Use concise, clear, simple language. Define unavoidable jargon before using it.
- Explain non-trivial designs and problems as: problem, concrete example or short trace, then solution. State why the solution is necessary and distinguish it from optional complexity.
- Prefer concrete behavior and small illustrations over abstract summaries, dense terminology, or unexplained lists of changes.
- When the user asks a question, answer it first before making edits or running implementation commands.
- When responding to user feedback or an analysis, explicitly say whether you agree or disagree before saying what you changed.

## Code Quality

### General

- Read files in full before wide-ranging changes, before editing files you have not fully inspected, and when asked to investigate or audit. Do not rely on search snippets for broad changes.
- Before recommending a fix design, check how sibling implementations (other adapters, providers, commands) handle the same concern, and follow that pattern. If you deviate, say why.
- Always ask before removing functionality or code that appears intentional.
- Inline single-line helpers that have only one call site.

### C#

- Methods always have bodies.
- All tests (e.g., in `Tests/` or `Tests.EndToEnd/`) must use the `ConceptName_StateUnderTest_ExpectedBehavior` naming scheme where `ConceptName` is the thing being tested (e.g., `FilmSubmission`), `StateUnderTest` is the condition the concept is subjected to (e.g., `CorruptedFilmFile`), and `ExpectedBehavior` is what is supposed to happen (e.g., `ValidationFails`).

### TypeScript

- No `any` in TypeScript unless absolutely necessary.
- No runtime checks of values whose types TypeScript already guarantees (no `typeof`/`instanceof`/`Array.isArray`/shape guards on typed parameters, typed callback results or typed declarations). Validate only at decided trust boundaries (wire input, schema-checked arguments and params).
- Check node_modules for external API types; don't guess.
- **No inline imports** (`await import()`, `import("pkg").Type`, dynamic type imports). Top-level imports only.
- Never remove or downgrade code to fix type errors from outdated deps; upgrade the dep instead.

## EndToEnd tests

- When writing end-to-end tests, prefer relying on the `data-testid` locator and CSS classes. Never rely on UI texts which are localized and can change without warning, breaking the test.

## User Override

If the user's instructions conflict with any rule in this document, ask for explicit confirmation before overriding. Only then execute their instructions.
