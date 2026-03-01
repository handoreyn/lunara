# ── Stage 1: build & publish ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy central build manifests and solution file first for layer caching.
COPY Directory.Build.props Directory.Packages.props Lunara.sln ./

# Copy all source projects (bin/obj excluded via .dockerignore).
COPY src/ src/

# Restore only the Worker and its transitive dependencies.
# NuGet package cache is mounted to speed up repeated builds.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/Platform/Lunara.Worker/Lunara.Worker.csproj

# Publish in Release mode, output to /app/publish.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/Platform/Lunara.Worker/Lunara.Worker.csproj \
      --configuration Release \
      --no-restore \
      --output /app/publish

# ── Stage 2: runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Copy published output from the build stage, owned by the non-root 'app' user
# that ships in the base image since .NET 8.
COPY --chown=app:app --from=build /app/publish .

# Explicitly run as non-root.
USER app

# Worker is a background service — no inbound ports.

ENTRYPOINT ["dotnet", "Lunara.Worker.dll"]
