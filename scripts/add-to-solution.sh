#!/bin/zsh
set -e

ROOT="/Users/fatih.gencaslan/Developer/lunara"
cd "$ROOT"

MODULES=(Identity Profiles Discovery Social Messaging Media Notifications Moderation)

# ── Add all projects to solution ───────────────────────────────────────────
echo "Adding platform projects to solution..."
dotnet sln add src/Platform/Lunara.Api/Lunara.Api.csproj \
               src/Platform/Lunara.Worker/Lunara.Worker.csproj \
               src/Platform/Lunara.SharedKernel/Lunara.SharedKernel.csproj \
               src/Platform/Lunara.BuildingBlocks/Lunara.BuildingBlocks.csproj \
               src/Platform/Lunara.Infrastructure.Host/Lunara.Infrastructure.Host.csproj

echo "Adding module projects to solution..."
for MOD in "${MODULES[@]}"; do
  dotnet sln add \
    "src/Modules/${MOD}/Lunara.${MOD}.Domain/Lunara.${MOD}.Domain.csproj" \
    "src/Modules/${MOD}/Lunara.${MOD}.Application/Lunara.${MOD}.Application.csproj" \
    "src/Modules/${MOD}/Lunara.${MOD}.Infrastructure/Lunara.${MOD}.Infrastructure.csproj"
  echo "  Added ${MOD} module projects"
done

echo ""
echo "Setting up project references..."

# ── Module internal references ─────────────────────────────────────────────
# Application -> Domain + SharedKernel
# Infrastructure -> Application + Domain
for MOD in "${MODULES[@]}"; do
  BASE="src/Modules/${MOD}"

  dotnet add "${BASE}/Lunara.${MOD}.Domain/Lunara.${MOD}.Domain.csproj" reference \
    "src/Platform/Lunara.SharedKernel/Lunara.SharedKernel.csproj"

  dotnet add "${BASE}/Lunara.${MOD}.Application/Lunara.${MOD}.Application.csproj" reference \
    "${BASE}/Lunara.${MOD}.Domain/Lunara.${MOD}.Domain.csproj" \
    "src/Platform/Lunara.SharedKernel/Lunara.SharedKernel.csproj" \
    "src/Platform/Lunara.BuildingBlocks/Lunara.BuildingBlocks.csproj"

  dotnet add "${BASE}/Lunara.${MOD}.Infrastructure/Lunara.${MOD}.Infrastructure.csproj" reference \
    "${BASE}/Lunara.${MOD}.Domain/Lunara.${MOD}.Domain.csproj" \
    "${BASE}/Lunara.${MOD}.Application/Lunara.${MOD}.Application.csproj"

  echo "  References set for ${MOD}"
done

# ── Infrastructure.Host references all module Application + Infrastructure ──
echo "Setting Infrastructure.Host references..."
INFRA_HOST_REFS=()
for MOD in "${MODULES[@]}"; do
  BASE="src/Modules/${MOD}"
  INFRA_HOST_REFS+=("${BASE}/Lunara.${MOD}.Application/Lunara.${MOD}.Application.csproj")
  INFRA_HOST_REFS+=("${BASE}/Lunara.${MOD}.Infrastructure/Lunara.${MOD}.Infrastructure.csproj")
done
dotnet add "src/Platform/Lunara.Infrastructure.Host/Lunara.Infrastructure.Host.csproj" reference \
  "${INFRA_HOST_REFS[@]}"

# ── Api and Worker reference Infrastructure.Host + SharedKernel ────────────
echo "Setting Api and Worker references..."
dotnet add "src/Platform/Lunara.Api/Lunara.Api.csproj" reference \
  "src/Platform/Lunara.Infrastructure.Host/Lunara.Infrastructure.Host.csproj" \
  "src/Platform/Lunara.SharedKernel/Lunara.SharedKernel.csproj"

dotnet add "src/Platform/Lunara.Worker/Lunara.Worker.csproj" reference \
  "src/Platform/Lunara.Infrastructure.Host/Lunara.Infrastructure.Host.csproj" \
  "src/Platform/Lunara.SharedKernel/Lunara.SharedKernel.csproj"

echo ""
echo "All done — solution and references configured."
