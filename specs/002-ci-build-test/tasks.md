# Tasks: Continuous Integration Build and Test Workflow

**Input**: Design documents from `/specs/002-ci-build-test/`
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [quickstart.md](quickstart.md)

**Organization**: Tasks are grouped by user story so the CI workflow, documentation, and governance increments can be implemented and reviewed independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the repository workflow location and confirm the current project/test layout before implementation.

- [X] T001 Create the GitHub Actions workflow directory at `.github/workflows/` and confirm `.github/workflows/build-and-test.yml` is the reserved workflow path.
- [X] T002 [P] Inspect `ContosoDashboard/ContosoDashboard.csproj` and `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj` to record the target framework and existing test project paths for CI commands.
- [X] T003 [P] Confirm the repository uses `main` as the pull request target branch in the CI workflow documentation and feature specification.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the validation command contract that all user stories depend on.

- [X] T004 Define the CI command sequence in `specs/002-ci-build-test/quickstart.md` as restore, build without restore, and test using the existing application and test project paths.
- [X] T005 [P] Verify the existing test project can be restored and discovered by `dotnet test` in `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj`.
- [X] T006 [P] Confirm the current .NET target framework is `net10.0` in `ContosoDashboard/ContosoDashboard.csproj` and document the SDK mapping used by CI in `specs/002-ci-build-test/plan.md`.

**Checkpoint**: The repository paths and local validation commands are known; user story implementation can begin.

---

## Phase 3: User Story 1 - Validate repository changes automatically (Priority: P1) 🎯 MVP

**Goal**: Run dependency restore, application build, and all existing tests on every push and on pull requests targeting `main`.

**Independent Test**: Validate the workflow YAML structure and run the same commands locally; a successful revision must pass all stages, while a deliberate build or test failure must fail the workflow with command output.

### Implementation for User Story 1

- [X] T007 [US1] Create `.github/workflows/build-and-test.yml` with `push` triggers for all branches and `pull_request` triggers targeting `main`.
- [X] T008 [US1] Configure `.github/workflows/build-and-test.yml` to use `ubuntu-latest`, `actions/checkout@v4`, and `actions/setup-dotnet@v4` with the SDK version matching `net10.0` from `ContosoDashboard/ContosoDashboard.csproj`.
- [X] T009 [US1] Add explicit restore, build, and test steps to `.github/workflows/build-and-test.yml` using `ContosoDashboard/ContosoDashboard.csproj` and `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj`, preserving failure output and step ordering.
- [X] T010 [US1] Validate `.github/workflows/build-and-test.yml` syntax and confirm its push and pull request event filters match FR-002 and FR-003 from `specs/002-ci-build-test/spec.md`.

**Checkpoint**: User Story 1 is complete when the workflow definition is valid and its local equivalent passes on a clean revision.

---

## Phase 4: User Story 2 - Understand and use the CI checks (Priority: P2)

**Goal**: Explain the CI workflow and give contributors reproducible local commands.

**Independent Test**: A contributor can read the README, identify the workflow triggers and stages, and run the documented restore, build, and test commands locally.

### Implementation for User Story 2

- [X] T011 [P] [US2] Add a Continuous Integration section to `README.md` describing `.github/workflows/build-and-test.yml`, push validation, pull request validation for `main`, and the restore/build/test stages.
- [X] T012 [P] [US2] Add the exact local validation commands to `README.md` for restoring `ContosoDashboard/ContosoDashboard.csproj`, building without restore, and testing `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj`.
- [X] T013 [US2] Document the expected failure behavior and explain how contributors can inspect failed command output in the GitHub Actions run logs in `README.md`.
- [X] T014 [US2] Cross-check the README CI instructions against `specs/002-ci-build-test/quickstart.md` and the workflow commands in `.github/workflows/build-and-test.yml`.

**Checkpoint**: User Story 2 is complete when the README accurately reproduces the workflow locally without undocumented steps.

---

## Phase 5: User Story 3 - Require passing CI before merging (Priority: P3)

**Goal**: Make passing CI an explicit repository governance requirement.

**Independent Test**: Review the constitution and confirm it states that changes with failing CI are not eligible for merge.

### Implementation for User Story 3

- [X] T015 [US3] Add a CI-before-merge principle to `.specify/memory/constitution.md` requiring restore, build, and test checks to pass before merge approval.
- [X] T016 [US3] Update the constitution sync impact report, semantic version, and last-amended date in `.specify/memory/constitution.md` for the new governance principle.
- [X] T017 [US3] Document in `.specify/memory/constitution.md` that GitHub branch protection must mark the CI workflow check as required for `main`, while keeping repository settings outside source-code implementation.

**Checkpoint**: User Story 3 is complete when the constitution and plan consistently describe CI as a merge gate.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verify the complete feature and preserve repository quality.

- [X] T018 [P] Validate YAML formatting and workflow paths in `.github/workflows/build-and-test.yml` with a local YAML parser or GitHub Actions-compatible validator.
- [X] T019 [P] Run `dotnet restore .\ContosoDashboard\ContosoDashboard.csproj` from the repository root and record the result for the CI quickstart in `specs/002-ci-build-test/quickstart.md`.
- [X] T020 [P] Run `dotnet build .\ContosoDashboard\ContosoDashboard.csproj --no-restore` and confirm the application compiles with the target framework documented in `README.md`.
- [X] T021 [P] Run `dotnet test .\ContosoDashboard.Tests\ContosoDashboard.Tests.csproj --no-restore` and confirm all existing tests pass before merge.
- [X] T022 Run `git diff --check` and review `README.md`, `.specify/memory/constitution.md`, and `.github/workflows/build-and-test.yml` for consistency with `specs/002-ci-build-test/spec.md`.
- [X] T023 Confirm the feature’s repository changes are limited to `.github/workflows/build-and-test.yml`, `README.md`, `.specify/memory/constitution.md`, and the CI feature documentation under `specs/002-ci-build-test/`.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; can start immediately.
- **Foundational (Phase 2)**: Depends on Setup; establishes target framework, project paths, and local commands.
- **User Story 1 (Phase 3)**: Depends on Foundational; delivers the MVP CI workflow.
- **User Story 2 (Phase 4)**: Depends on User Story 1’s command contract; documents the implemented workflow.
- **User Story 3 (Phase 5)**: Depends on User Story 1’s workflow name and behavior; formalizes the merge requirement.
- **Polish (Phase 6)**: Depends on the desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 2 and is the MVP; independent of documentation and governance edits except for shared workflow naming.
- **User Story 2 (P2)**: Depends on the final workflow commands from User Story 1, but can be developed in parallel after those commands are agreed.
- **User Story 3 (P3)**: Depends on the final workflow check name from User Story 1; otherwise independent of README implementation.

### Parallel Opportunities

- T002 and T003 can run in parallel during Setup.
- T005 and T006 can run in parallel during Foundational work.
- T011 and T012 can run in parallel within User Story 2 because they update separate README sections or adjacent documentation blocks.
- T015 and T017 can be drafted in parallel, then reconciled by T016.
- T018 through T021 can run in parallel after the implementation files exist.

---

## Parallel Example: User Story 1

```text
Task A: Configure workflow triggers and runner in .github/workflows/build-and-test.yml
Task B: Confirm restore/build/test command paths from the project and test csproj files
Task C: Validate workflow YAML syntax and event filters
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate the workflow locally with restore, build, and test commands.
5. Confirm the workflow file is ready for a push or pull request to `main`.

### Incremental Delivery

1. Setup + Foundational: establish the command and target framework contract.
2. User Story 1: add automated GitHub Actions validation as the MVP.
3. User Story 2: document the workflow and local reproduction steps.
4. User Story 3: update constitution governance and branch protection guidance.
5. Polish: validate YAML, local commands, documentation, and diff consistency.

### Parallel Team Strategy

1. Complete Setup and Foundational tasks together.
2. Developer A implements User Story 1 in `.github/workflows/build-and-test.yml`.
3. Developer B prepares README documentation after the workflow command contract is agreed.
4. Developer C prepares the constitution amendment and branch protection guidance.
5. Complete cross-cutting validation before merge.
