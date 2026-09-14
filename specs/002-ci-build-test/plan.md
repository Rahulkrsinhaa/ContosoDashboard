# Implementation Plan: Continuous Integration Build and Test Workflow

**Branch**: `002-ci-build-test` | **Date**: 2026-09-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-ci-build-test/spec.md`

## Summary

Add a single GitHub Actions workflow that validates the repository on every push and on pull requests targeting `main`. The workflow will select the SDK required by the application target framework, restore dependencies, build the main application, and run all discovered tests. README documentation will explain the workflow and equivalent local commands, while the constitution will gain a CI-before-merge governance principle.

## Technical Context

**Language/Version**: C# / .NET 10.0, derived from `ContosoDashboard/ContosoDashboard.csproj`; CI uses the matching `10.0.x` SDK band  
**Primary Dependencies**: GitHub Actions, `actions/checkout`, `actions/setup-dotnet`, .NET CLI  
**Storage**: N/A; CI uses the repository source tree and hosted runner workspace  
**Testing**: `dotnet test` across the repository; existing xUnit test project discovery  
**Target Platform**: GitHub-hosted Ubuntu runner; local Windows development remains supported  
**Project Type**: Web application repository with a Blazor Server project and test project  
**Performance Goals**: Complete normal restore, build, and test validation within a practical CI run; no new runtime performance target  
**Constraints**: preserve offline-first application behavior, derive SDK selection from the project target, expose command failures, avoid secrets and external application services  
**Scale/Scope**: one application project, existing test projects, one workflow, one README section, and one constitution amendment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Security-First for Training**: PASS — the workflow requires no application credentials or secrets and only executes repository validation commands.
- **Offline-First, Cloud-Ready Architecture**: PASS — CI validates the local .NET application and does not add cloud runtime dependencies.
- **Testable and Incremental Delivery**: PASS — the workflow directly automates restore, build, and test checks and makes failures visible.
- **User-Scoped Data Integrity**: PASS — no user data or authorization behavior is changed.
- **Simplicity, Clarity, and Maintainability**: PASS — one focused workflow and concise documentation follow existing repository conventions.

## Phase 0: Research Decisions

- Use `actions/checkout@v4` and `actions/setup-dotnet@v4` on `ubuntu-latest`.
- Read the `net10.0` target framework from the project file as the source of truth and configure `10.0.x` with `actions/setup-dotnet` in the workflow.
- Run `dotnet restore`, `dotnet build --no-restore`, and `dotnet test --no-build` so CI stages are explicit and failures identify the responsible phase.
- Keep test execution tolerant of the repository’s existing test-project layout by using a repository-level test command or an explicit solution/project path discovered during implementation.
- Document that branch protection is configured in GitHub repository settings; the constitution expresses the policy but cannot enforce settings by itself.

## Phase 1: Design

### Repository Changes

```text
.github/workflows/build-and-test.yml  # New push/PR validation workflow
README.md                             # New CI section and local commands
.specify/memory/constitution.md       # New CI-before-merge principle and version update
```

### Workflow Contract

- Trigger `push` for all branches.
- Trigger `pull_request` only when the base branch is `main`.
- Checkout the repository.
- Set up the SDK matching the application target framework.
- Restore dependencies.
- Build the main project without repeating restore.
- Run all existing automated tests after a successful build.
- Let command failures fail the job and preserve command output in the Actions log.

### Documentation Contract

README documentation will identify the workflow path, trigger behavior, validation stages, local commands, and the requirement that a clean local run should precede a pull request.

### Governance Contract

The constitution will add a concise Development Workflow principle stating that all changes must pass the CI workflow before merge, with a minor version increment and an updated sync impact report.

## Constitution Check After Design

- **Security-First for Training**: PASS — no secrets, credentials, or application access are introduced.
- **Offline-First, Cloud-Ready Architecture**: PASS — the workflow validates existing local application behavior without changing runtime dependencies.
- **Testable and Incremental Delivery**: PASS — each CI stage maps to a repeatable local command and acceptance scenario.
- **User-Scoped Data Integrity**: PASS — no business data paths are modified.
- **Simplicity, Clarity, and Maintainability**: PASS — changes are limited to one workflow and two documentation/governance files.

## Complexity Tracking

No constitution violations require justification for this feature.
