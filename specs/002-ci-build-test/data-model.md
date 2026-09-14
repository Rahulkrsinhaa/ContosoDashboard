# Data Model: Continuous Integration Build and Test Workflow

This feature does not add application data entities or database schema changes.

## Repository Artifacts

| Artifact | Purpose | Owner |
|---|---|---|
| `.github/workflows/build-and-test.yml` | Defines automated validation triggers and commands | Repository maintainers |
| `README.md` | Explains CI behavior and local reproduction commands | Contributors |
| `.specify/memory/constitution.md` | Governs the CI-before-merge requirement | Project maintainers |

## Relationships

- The workflow reads the target framework from `ContosoDashboard/ContosoDashboard.csproj` as its SDK compatibility input.
- The workflow executes against the application project and discovers the existing test project through the .NET CLI.
- README instructions describe the same validation sequence implemented by the workflow.
- The constitution defines the governance expectation that the workflow passes before merge.

## Validation Rules

- Pushes trigger validation for every branch.
- Pull requests trigger validation only when the base branch is `main`.
- Build must complete successfully before tests run.
- A failed restore, build, or test command fails the workflow.
- No application database records, migrations, or persistent entities are introduced.
