# Git Flow

Lunara follows a simplified **master / develop** branching model.

## Branches

| Branch          | Role |
|-----------------|------|
| `master`        | Production-ready code. Only receives merges from `develop` (or hotfix branches). Protected — no direct pushes. |
| `develop`       | Integration branch. All feature and fix branches merge here. CI runs on every push. |
| `feature/<name>` | New features. Branched from `develop`, merged back to `develop` via PR. |
| `fix/<name>`    | Bug fixes targeting `develop`. |
| `hotfix/<name>` | Critical production fixes. Branched from `master`, merged into both `master` and `develop`. |
| `release/<sem>` | Optional release-prep branch (e.g. `release/1.2.0`). Branched from `develop`, merged into `master` + `develop`. |

## Workflow

### Feature development

```bash
# Start work
git checkout develop
git pull origin develop
git checkout -b feature/identity-jwt

# ... develop and commit (Conventional Commits) ...

# Bring in latest develop before opening PR
git fetch origin
git rebase origin/develop

# Push and open PR -> develop
git push origin feature/identity-jwt
```

### Release to master

```bash
# After develop is stable and tested, open PR: develop -> master
# Merge commit message:
#   chore(repo): release v1.0.0

# Tag after merge
git checkout master
git pull origin master
git tag -a v1.0.0 -m "release: v1.0.0"
git push origin v1.0.0
```

### Hotfix

```bash
git checkout master
git pull origin master
git checkout -b hotfix/fix-auth-race

# ... fix and commit ...

# Merge into master
git checkout master
git merge --no-ff hotfix/fix-auth-race
git tag -a v1.0.1 -m "release: v1.0.1"

# Merge into develop
git checkout develop
git merge --no-ff hotfix/fix-auth-race

git branch -d hotfix/fix-auth-race
```

## Pull Request Rules

- PRs require at least 1 approving review.
- All CI checks must pass before merge.
- Use **Squash and merge** for feature branches to keep history clean.
- Use **Merge commit** for release / hotfix branches to preserve the release boundary.
- Delete the source branch after merge.

## Default Branch

`master` is the default and protected branch.
