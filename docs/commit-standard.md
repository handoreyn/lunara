# Commit Standard

Lunara uses **Conventional Commits v1.0.0** (<https://www.conventionalcommits.org>).

## Format

```
<type>(<scope>): <short summary>

[optional body]

[optional footer(s)]
```

- **Header** must not exceed **72 characters**.
- **Body** lines must not exceed **100 characters**.
- Use the **imperative mood** in the summary ("add", not "added" or "adds").

## Types

| Type       | When to use |
|------------|-------------|
| `feat`     | New feature visible to users or downstream consumers |
| `fix`      | Bug fix |
| `refactor` | Code restructuring without feature/fix |
| `perf`     | Performance improvement |
| `test`     | Adding or correcting tests |
| `build`    | Build system / dependency changes |
| `ci`       | CI/CD configuration changes |
| `chore`    | Maintenance tasks (scaffolding, tooling) |
| `docs`     | Documentation only |
| `style`    | Formatting, whitespace (no logic change) |
| `revert`   | Reverting a previous commit |

## Scopes

Use the module or platform project name as scope where applicable.

| Scope            | Applies to |
|------------------|------------|
| `repo`           | Repository-level changes (CI, config, structure) |
| `identity`       | Identity module |
| `profiles`       | Profiles module |
| `discovery`      | Discovery module |
| `social`         | Social module |
| `messaging`      | Messaging module |
| `media`          | Media module |
| `notifications`  | Notifications module |
| `moderation`     | Moderation module |
| `api`            | Lunara.Api platform project |
| `worker`         | Lunara.Worker platform project |
| `shared`         | Lunara.SharedKernel or BuildingBlocks |
| `host`           | Lunara.Infrastructure.Host |
| `arch`           | Architecture / arch tests |
| `ci`             | CI/CD pipeline |

## Examples

```
feat(identity): add JWT token issuance endpoint

fix(messaging): correct message fanout routing key

test(arch): add application-must-not-reference-infrastructure rule

chore(repo): scaffold clean architecture platform and module projects
```

## Breaking Changes

Add `!` after the type/scope, and include a `BREAKING CHANGE:` footer:

```
feat(identity)!: remove legacy token endpoint

BREAKING CHANGE: The /auth/v1/token endpoint has been removed.
Use /auth/v2/tokens instead.
```
