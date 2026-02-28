## Release / Hotfix Summary
- Release version: `vX.Y.Z`
- Base branch: (choose one)
  - [ ] develop -> master (release)
  - [ ] hotfix/* -> master (hotfix)
- Related issues/PRs:

## Release checklist
- [ ] CI is green (build + unit + arch)
- [ ] No pending migrations/DB changes unreviewed (if applicable)
- [ ] Config changes reviewed (appsettings/env)
- [ ] Version/tag plan agreed

## Test evidence
Commands run:
- [ ] `dotnet build Lunara.sln -c Release`
- [ ] `dotnet test Lunara.sln -c Release`

Manual checks (if applicable):
- [ ] API smoke endpoints ok (`/health`, `/ready`)
- [ ] Worker starts without errors
- [ ] Outbox publisher works (if included in release)

## Tagging (after merge to master)
- [ ] Create annotated tag: `vX.Y.Z`
- [ ] Push tag to origin

## Rollback plan
- What is the rollback strategy if something goes wrong?