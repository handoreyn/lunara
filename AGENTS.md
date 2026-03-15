# Lunara Agent Policy

## 1. Repository Context

- Lunara uses Clean Architecture + DDD
- .NET 10
- Each module has Domain / Application / Infrastructure projects
- warnings-as-errors must remain green
- unit tests and architectural tests must remain green
- no AutoMapper, no MediatR, no MassTransit unless explicitly requested

## 2. Branching Rules

- All normal work starts from `develop`
- Every task must use a side branch from `develop`
- Allowed branch names:
  - `feature/<short-name>`
  - `fix/<short-name>`
  - `hotfix/<short-name>`
- Feature/fix PRs must target `develop`
- Never open feature/fix PRs to `master`

## 3. Atomic Work Rules

- Work in small atomic steps
- Do not mix unrelated concerns in one step
- After each atomic step:
  1. run `dotnet build Lunara.sln -c Release`
  2. run `dotnet test Lunara.sln -c Release`
  3. create one Conventional Commit
- Prefer one logical concern per commit

## 4. PR Rules

- At the end of a completed task:
  - push the branch
  - create a PR to `develop`
- Use the correct PR template:
  - feature/fix → `.github/PULL_REQUEST_TEMPLATE/feature.md`
  - release → `.github/PULL_REQUEST_TEMPLATE/release.md`
  - hotfix → `.github/PULL_REQUEST_TEMPLATE/hotfix.md`

## 5. Git Safety Rules

- Before destructive or history-changing git commands, ask for explicit confirmation
- This includes:
  - `git reset`
  - `git rebase`
  - `git push --force`
  - deleting branches
  - merging to protected branches
- Never bypass protected branch workflow

## 6. Agent Execution Rules

- For each task, the agent must:
  1. checkout `develop`
  2. pull latest changes
  3. create the correct side branch
  4. implement the task in atomic steps
  5. run build and tests after each step
  6. commit with Conventional Commits
  7. push the branch
  8. create a PR to `develop`
- If the environment does not allow direct git or PR actions, the agent must provide:
  - exact git commands
  - exact commit messages
  - exact PR title
  - exact PR body

## 7. Reporting Rules

- After each step, report:
  - changed files
  - build/test status
  - commit message used
- After task completion, report:
  - final branch name
  - commits created
  - PR title
  - PR target branch
  - PR URL if available