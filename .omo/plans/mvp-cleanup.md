# mvp-cleanup - Work Plan

## TL;DR (For humans)

**What you'll get:** A tidied PITS repo — 8 empty/skeleton .NET projects removed from the root `src/` and `tests/` folders, 3 empty directories deleted, the `dist/` publish output deleted and gitignored, and the misnamed `Dosc/` folder fixed (blueprint doc moved to `docs/`, placeholder Readme removed). Everything still compiles and runs exactly as before.

**Why this approach:** Incremental directory removals are verified by `dotnet build` after each wave, so a broken reference is caught immediately. The shell projects were orphaned (not in any .sln) so removal is purely additive — no build configs to update.

**What it will NOT do:** Touch any `mvp/` code, POC projects, `.trae/` design specs, `mvp-art/` assets, or the 12 UI screenshots the user chose to keep. It only cleans the root-level scaffolding that was never wired into the active build.

**Effort:** Quick
**Risk:** Low — all targets are orphaned files not referenced by any .sln or build pipeline
**Decisions to sanity-check:** None — all confirmed by user approval on 2026-07-10

Your next move: Run `$start-work` to execute.

---

> TL;DR (machine): Quick | Low | Delete 8 empty/skeleton .NET projects (6× src/, 2× tests/), 3 empty dirs, dist/; fix Dosc/; update .gitignore; verify `dotnet build` still passes.

## Scope
### Must have
- Delete `src/PITS.Core/`, `src/PITS.Infrastructure/`, `src/PITS.AI/`, `src/PITS.API/`, `src/PITS.CLI/`, `src/PITS.TUI/` — 6 empty/skeleton projects (not in PITS.sln)
- Delete `tests/PITS.Core.Tests/`, `tests/PITS.Integration.Tests/` — 2 empty test projects (not in PITS.sln)
- Delete `.uploads/`, `.agents/` — empty directories
- Delete `dist/` — publish artifact
- Add `dist/` to `.gitignore`
- Fix `Dosc/`: delete `Dosc/Readme.md`, move `Dosc/PITS-全案蓝图-统一版.md` to `docs/PITS-全案蓝图-统一版.md`, then delete `Dosc/`
- Verify `dotnet build` succeeds for both `PITS.sln` and `mvp/PITS.MVP.sln`

### Must NOT have (guardrails, anti-slop, scope boundaries)
- **Must NOT** modify any file under `mvp/` (code, POCs, tests, assets)
- **Must NOT** delete `.trae/`, `.codex/`, `.codegraph/`, `.vs/`
- **Must NOT** delete `mvp-art/`, `svp-art/`, or the 12 UI screenshots (`pits-ui-*.png`)
- **Must NOT** touch any `.csproj` file of the active MVP projects
- **Must NOT** modify any production C#/XAML source code

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: none (cleanup task — no logic to test)
- Evidence: `.omo/evidence/task-1-mvp-cleanup.dry-run.txt`, `.omo/evidence/task-1-mvp-cleanup.build-verify.txt`
- Each delete is verified by `Test-Path` before and after; final verification is `dotnet build` on both .sln files

## Execution strategy
### Parallel execution waves
- **Wave 1** — Delete 6 empty `src/` projects (parallelizable within wave)
- **Wave 2** — Delete 2 empty `tests/` projects + `.uploads/` + `.agents/` + `dist/`
- **Wave 3** — Fix `Dosc/` + update `.gitignore` + final build verification

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| 1 (delete src/Core + src/Infrastructure) | — | — | 2, 3 |
| 2 (delete src/AI + src/API) | — | — | 1, 3 |
| 3 (delete src/CLI + src/TUI) | — | — | 1, 2 |
| 4 (delete tests/* + .uploads/ + .agents/ + dist/) | — | — | 1, 2, 3 |
| 5 (fix Dosc/) | — | — | 1, 2, 3, 4 |
| 6 (update .gitignore) | — | — | 1, 2, 3, 4, 5 |
| 7 (build verify) | 1, 2, 3, 4, 5, 6 | — | — |

## Todos
> Implementation + Test = ONE todo. Never separate.
<!-- APPEND TASK BATCHES BELOW THIS LINE WITH edit/apply_patch - never rewrite the headers above. -->
- [x] 1. Delete `src/PITS.Core/` and `src/PITS.Infrastructure/`
  What to do / Must NOT do: Remove the two directories recursively. These projects are NOT referenced in PITS.sln (confirmed: PITS.sln only references `mvp/src/PITS.MVP.Core` and `mvp/src/PITS.MVP.Infrastructure` under the `mvp/` prefix). Must NOT touch the `mvp/src/PITS.MVP.Core/` or `mvp/src/PITS.MVP.Infrastructure/` directories.
  Parallelization: Wave 1 | Blocked by: — | Blocks: 7
  References: `PITS.sln` lines 9-12 (only references mvp/ projects), `src/PITS.Core/Class1.cs`, `src/PITS.Infrastructure/Class1.cs`
  Acceptance criteria: `Test-Path "src/PITS.Core"` returns False; `Test-Path "src/PITS.Infrastructure"` returns False
  QA scenarios: happy — `Remove-Item -Recurse -Force src/PITS.Core; Remove-Item -Recurse -Force src/PITS.Infrastructure; Test-Path "src/PITS.Core"` → False. failure — not applicable (no lock risk).
  Evidence: `.omo/evidence/task-1-mvp-cleanup.dry-run.txt`
  Commit: N | squashed into final commit

- [x] 2. Delete `src/PITS.AI/` and `src/PITS.API/`
  What to do / Must NOT do: Remove the two directories. PITS.AI is a skeleton with only Class1.cs + stale NuGet refs to SemanticKernel/OllamaSharp (target net8.0). PITS.API is a bare `CreateBuilder` + `Run()`. Neither is in any .sln. Must NOT touch `mvp/poc/PITS.POC.AI/`.
  Parallelization: Wave 1 | Blocked by: — | Blocks: 7
  References: `src/PITS.AI/Program.cs`, `src/PITS.API/Program.cs`
  Acceptance criteria: `Test-Path "src/PITS.AI"` → False; `Test-Path "src/PITS.API"` → False
  QA scenarios: happy — `Remove-Item -Recurse -Force src/PITS.AI, src/PITS.API` then verify not exist
  Evidence: `.omo/evidence/task-2-mvp-cleanup.dry-run.txt`
  Commit: N

- [x] 3. Delete `src/PITS.CLI/` and `src/PITS.TUI/`
  What to do / Must NOT do: Remove the two directories. Both contain only a single `Console.WriteLine("... Coming Soon")` Program.cs. Phase 3 features — safe to delete.
  Parallelization: Wave 1 | Blocked by: — | Blocks: 7
  References: `src/PITS.CLI/Program.cs`, `src/PITS.TUI/Program.cs`
  Acceptance criteria: `Test-Path "src/PITS.CLI"` → False; `Test-Path "src/PITS.TUI"` → False
  QA scenarios: happy — `Remove-Item -Recurse -Force src/PITS.CLI, src/PITS.TUI` then verify
  Evidence: `.omo/evidence/task-3-mvp-cleanup.dry-run.txt`
  Commit: N

- [x] 4. Delete empty test projects, empty dirs, and dist/
  What to do / Must NOT do: Delete `tests/PITS.Core.Tests/`, `tests/PITS.Integration.Tests/` (not in PITS.sln — real tests are in `mvp/tests/`). Delete `.uploads/`, `.agents/`. Delete `dist/` entirely (publish artifact, user confirmed delete). Must NOT touch `mvp/tests/` directory.
  Parallelization: Wave 2 | Blocked by: — | Blocks: 7
  References: `tests/PITS.Core.Tests/UnitTest1.cs`, `tests/PITS.Integration.Tests/UnitTest1.cs`, `.uploads/` (empty), `.agents/` (empty), `dist/PITS-MVP-20260702.zip`
  Acceptance criteria: All 5 paths not exist after removal
  QA scenarios: happy — batch remove all 5, `Test-Path` each → all False
  Evidence: `.omo/evidence/task-4-mvp-cleanup.dry-run.txt`
  Commit: N

- [x] 5. Fix `Dosc/` — move blueprint doc, delete Readme
  What to do / Must NOT do: Create `docs/` directory (if not exist). Copy `Dosc/PITS-全案蓝图-统一版.md` to `docs/PITS-全案蓝图-统一版.md`. Delete `Dosc/Readme.md` (content is "123"). Delete empty `Dosc/` directory. Must NOT lose the blueprint document.
  Parallelization: Wave 3 | Blocked by: — | Blocks: 7
  References: `Dosc/Readme.md`, `Dosc/PITS-全案蓝图-统一版.md`
  Acceptance criteria: `Test-Path "docs/PITS-全案蓝图-统一版.md"` → True; `Test-Path "Dosc"` → False
  QA scenarios: happy — create dir, copy file, delete old Readme, delete old dir, verify new path exists and old path doesn't
  Evidence: `.omo/evidence/task-5-mvp-cleanup.dry-run.txt`
  Commit: N

- [x] 6. Add `dist/` to `.gitignore`
  What to do / Must NOT do: Append `/dist/` entry to root `.gitignore`. Must NOT change any existing entries.
  Parallelization: Wave 3 | Blocked by: — | Blocks: 7
  References: `.gitignore` line 97 (last line)
  Acceptance criteria: `.gitignore` contains `/dist/` on a new line at the end
  QA scenarios: happy — append `/dist/` to `.gitignore`, `Select-String -Pattern "^/dist/$" .gitignore` → match found
  Evidence: `.omo/evidence/task-6-mvp-cleanup.dry-run.txt`
  Commit: N

- [x] 7. Build verification — both solutions compile
  What to do / Must NOT do: Run `dotnet build` on `PITS.sln` and `mvp/PITS.MVP.sln`. Both must succeed (exit code 0). Must NOT run on any other projects. Must NOT use `--no-restore` (ensure NuGet restore works).
  Parallelization: Wave 4 | Blocked by: 1, 2, 3, 4, 5, 6 | Blocks: —
  References: `PITS.sln`, `mvp/PITS.MVP.sln`
  Acceptance criteria: Both `dotnet build` commands exit with code 0, no build errors
  QA scenarios: happy — `dotnet build PITS.sln 2>&1 | Select-String -Pattern "Build succeeded"` → matched twice (both projects). failure — if build fails, output the error, stop and report.
  Evidence: `.omo/evidence/task-7-mvp-cleanup.build-verify.txt`
  Commit: Y | `chore(root): remove empty/skeleton projects and dirs, fix Dosc/`

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [x] F1. Plan compliance audit
- [x] F2. Code quality review
- [x] F3. Real manual QA — verify `dotnet build` still passes, check git status shows only expected deletions
- [x] F4. Scope fidelity — confirm no mvp/ files were touched

## Commit strategy
- Squash commits from todos 1-6 into a single commit with message:
  `chore(root): remove empty/skeleton projects and dirs, fix Dosc/`
- Todo 7 build verification happens before committing
- Push after user approve final verification

## Success criteria
1. `src/PITS.*/` (6 directories) no longer exist
2. `tests/PITS.*/` (2 directories) no longer exist
3. `.uploads/`, `.agents/`, `dist/` no longer exist
4. `docs/PITS-全案蓝图-统一版.md` exists, `Dosc/` no longer exists
5. `.gitignore` contains `/dist/`
6. `dotnet build PITS.sln` succeeds
7. `dotnet build mvp/PITS.MVP.sln` succeeds
8. `git status` shows only expected deletions (no mvp/ files changed)
