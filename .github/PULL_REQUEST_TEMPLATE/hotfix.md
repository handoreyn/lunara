## Hotfix Summary
- Hotfix version: `vX.Y.Z` (usually patch)
- Base: `hotfix/*` -> `master`
- Issue/incident reference:
- Why is this urgent?

## Change checklist
- [ ] Minimal change (only what’s needed for the fix)
- [ ] No refactors mixed in
- [ ] Risk assessed

## Test evidence
Commands run:
- [ ] `dotnet build Lunara.sln -c Release`
- [ ] `dotnet test Lunara.sln -c Release`

Manual checks (if applicable):
- [ ] Reproduced the bug before fix
- [ ] Verified fix after change
- [ ] API endpoints ok (`/health`, `/ready`)
- [ ] Worker ok (if affected)

## Merge-back checklist
After merging to `master`:
- [ ] Open PR or merge `hotfix/*` back into `develop`
- [ ] Ensure version/tag is aligned in `develop`

## Tagging (after merge to master)
- [ ] Create annotated tag: `vX.Y.Z`
- [ ] Push tag to origin