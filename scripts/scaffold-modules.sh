#!/bin/zsh
set -e

ROOT="/Users/fatih.gencaslan/Developer/lunara"
cd "$ROOT"

MODULES=(Identity Profiles Discovery Social Messaging Media Notifications Moderation)

# ── Module projects ────────────────────────────────────────────────────────
for MOD in "${MODULES[@]}"; do
  BASE="src/Modules/${MOD}"

  for LAYER in Domain Application Infrastructure; do
    PROJ="Lunara.${MOD}.${LAYER}"
    OUT="${BASE}/${PROJ}"

    if [ ! -d "$OUT" ]; then
      dotnet new classlib -n "$PROJ" -o "$OUT" --no-restore
    fi
    rm -f "${OUT}/Class1.cs"
  done

  # Domain folders
  mkdir -p "${BASE}/Lunara.${MOD}.Domain/Entities"          \
           "${BASE}/Lunara.${MOD}.Domain/ValueObjects"       \
           "${BASE}/Lunara.${MOD}.Domain/DomainEvents"       \
           "${BASE}/Lunara.${MOD}.Domain/Repositories"

  # Application folders
  mkdir -p "${BASE}/Lunara.${MOD}.Application/UseCases"     \
           "${BASE}/Lunara.${MOD}.Application/Ports"         \
           "${BASE}/Lunara.${MOD}.Application/DTOs"

  # Infrastructure folders
  mkdir -p "${BASE}/Lunara.${MOD}.Infrastructure/Persistence" \
           "${BASE}/Lunara.${MOD}.Infrastructure/Messaging"   \
           "${BASE}/Lunara.${MOD}.Infrastructure/DependencyInjection"

  # add .gitkeep to keep empty dirs
  find "${BASE}" -type d | xargs -I{} sh -c '[ -z "$(ls -A "{}")" ] && touch "{}/.gitkeep" || true'

  echo "Module ${MOD} scaffolded"
done

# ── Platform sub-folders ──────────────────────────────────────────────────
mkdir -p src/Platform/Lunara.SharedKernel/Domain        \
         src/Platform/Lunara.SharedKernel/Application   \
         src/Platform/Lunara.BuildingBlocks/Extensions  \
         src/Platform/Lunara.BuildingBlocks/Guards       \
         src/Platform/Lunara.Infrastructure.Host/Modules

find src/Platform/Lunara.SharedKernel src/Platform/Lunara.BuildingBlocks src/Platform/Lunara.Infrastructure.Host -type d | \
  xargs -I{} sh -c '[ -z "$(ls -A "{}")" ] && touch "{}/.gitkeep" || true'

echo "Platform sub-folders created"
