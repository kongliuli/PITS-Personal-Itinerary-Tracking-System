# MVP PR readiness design

This branch should close the MVP as a verifiable product slice, not expand the
future platform.

## Scope

- Treat `PITS.sln` as the PR gate.
- Treat `mvp/PITS.MVP.sln` as the local MVP solution.
- Keep POC projects as optional demos only; they must not add security warnings
  or block MVP validation.
- Keep `mvp-art/`, `svp-art/`, and `.trae/` as design/demo assets.

## Implementation

1. Security
   - Override vulnerable `SQLitePCLRaw` transitive packages with a patched
     `SQLitePCLRaw.bundle_e_sqlite3`.
   - Remove the unused Semantic Kernel dependency from the AI POC.
   - Remove the unused EF design package from the storage POC.

2. MVP completeness
   - Fix App converters so binding reverse paths do not throw.
   - Keep the current lightweight AI assistant; do not add an LLM dependency for
     MVP.

3. Structure and docs
   - Remove the duplicate `mvp/src/PITS.MVP.sln`.
   - Update the root README to point at the real MVP entry points and validation
     commands.

## Verification

- `dotnet list PITS.sln package --vulnerable --include-transitive`
- `dotnet restore PITS.sln`
- `dotnet build mvp/src/PITS.MVP.App/PITS.MVP.App.csproj -f net10.0-windows10.0.19041.0 --no-restore`
- `dotnet test mvp/tests/PITS.MVP.Core.Tests/PITS.MVP.Core.Tests.csproj --no-restore`
- `dotnet test mvp/tests/PITS.MVP.Infrastructure.Tests/PITS.MVP.Infrastructure.Tests.csproj --no-restore`

Full `dotnet build PITS.sln --no-restore` still depends on Android SDK being
installed locally because the MAUI App targets Android.
