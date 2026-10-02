---
name: commit-changes
description: >-
  Commits uncommitted work in the MAUI git root, split by kind of change.
  Use when the user says "Commit MAUI changes", "Commit Maui changes",
  or asks to commit MAUI work with separated commits.
---

# Commit MAUI changes

## Triggers

- `Commit MAUI changes` / `Commit Maui changes`
- Similar phrasing that names this MAUI repo and asks to commit

Do not fold Flutter or backend commits into this skill (those are separate git roots).

## Repo root

Use `git -C <absolute-path-to-maui-root> …` (this repo is `/Users/set/Desktop/SubconsciousET/Projects/maui` or the workspace’s maui root). Do not rely on Shell `working_directory` alone when other roots are open.

Do not commit Flutter (`flutter-frontend-of-principles`) or backend unless the user names those repos.

## Authorship (required)

- Never add `Co-authored-by`, Cursor/AI trailers, or any contributor attribution for the agent.
- Never pass `--author`, never change `user.name` / `user.email`, never amend to rewrite author.
- Commits must use the existing local git identity only (today: Bohdan Bats).

## How to commit

1. Status, full diff, and recent `git log` in this repo only (`git -C …`).
2. Group into **separate commits by kind** (feature vs fix vs refactor vs tests vs docs/rules vs config). Prefer focused commits over one dump. Keep a feature and its tests together when they are one unit.
3. Stage only files for the current commit; use HEREDOC messages in this repo’s style (imperative, why-focused, ~1–2 sentences).
4. No secrets (`.env`, real API keys, app passwords, keystores). Warn and skip those files. Prefer committing `.env.example` only when it has placeholders.
5. Skip **local development only** changes (machine-specific paths, personal launch settings, local debug toggles, scratch/tooling not meant for the shared repo). Leave them unstaged and mention them briefly after committing.
6. Do not push unless asked.
7. After all commits: `git status` and briefly list the new commit subjects.

Follow the user’s global git safety rules (no force push, no amend unless those rules allow it, no `--no-verify`).
