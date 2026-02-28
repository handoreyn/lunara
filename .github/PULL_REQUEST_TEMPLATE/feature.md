## Summary
- What does this PR change?

## Type
- [ ] feat
- [ ] fix
- [ ] refactor
- [ ] test
- [ ] docs
- [ ] chore / build / ci

## Scope
- [ ] identity
- [ ] profiles
- [ ] discovery
- [ ] social
- [ ] messaging
- [ ] media
- [ ] notifications
- [ ] moderation
- [ ] api
- [ ] worker
- [ ] host
- [ ] shared / bb
- [ ] repo / ci / docs

## How to test
Commands run:
- [ ] `dotnet build Lunara.sln -c Release`
- [ ] `dotnet test Lunara.sln -c Release`

Manual checks (if applicable):
- [ ] endpoint tested (curl / Postman)
- [ ] worker logs checked
- [ ] outbox row created & published (if relevant)

## Architecture checklist
- [ ] Clean Architecture boundaries respected (Domain ↛ Infrastructure, Application ↛ Infrastructure)
- [ ] No forbidden frameworks (AutoMapper/MediatR/MassTransit)
- [ ] Unit tests added/updated

## Notes for reviewers
- Anything reviewers should pay attention to?