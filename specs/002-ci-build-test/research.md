# Research: Continuous Integration Build and Test Workflow

## Decision: Use GitHub-hosted Ubuntu with standard Actions

- **Decision**: Use `ubuntu-latest`, `actions/checkout@v4`, and `actions/setup-dotnet@v4`.
- **Rationale**: The repository is a cross-platform .NET application, and Ubuntu runners provide supported .NET tooling without requiring SQL Server LocalDB for restore/build/test. The workflow remains independent of the application’s local database runtime.
- **Alternatives considered**: Windows runners were not selected because they add startup cost and are unnecessary for the requested .NET validation; self-hosted runners were rejected because they introduce maintenance and credential concerns.

## Decision: Make SDK selection follow the project target

- **Decision**: Configure the workflow for the .NET major version represented by `TargetFramework` in `ContosoDashboard/ContosoDashboard.csproj`, currently `net10.0`.
- **Rationale**: This keeps CI aligned with the project’s declared build contract and makes target-framework changes visible during workflow maintenance.
- **Alternatives considered**: Hard-coding .NET 8 was rejected because the repository currently targets .NET 10; installing multiple SDKs was rejected because it adds unnecessary complexity.

## Decision: Keep restore, build, and test as separate stages

- **Decision**: Run explicit `dotnet restore`, `dotnet build --no-restore`, and `dotnet test --no-build` commands.
- **Rationale**: Separate commands provide clear failure boundaries and match the assignment’s required workflow stages.
- **Alternatives considered**: A single `dotnet test` command was rejected because it hides the requested restore/build stages; custom scripts were rejected because the repository does not need another abstraction for three standard CLI commands.

## Decision: Document branch protection separately from workflow triggers

- **Decision**: Document that the workflow runs on pushes and pull requests, while requiring administrators to mark the check as required in GitHub branch protection settings.
- **Rationale**: A workflow can report status but cannot itself enforce merge restrictions.
- **Alternatives considered**: Repository settings automation was rejected as out of scope and would require permissions and configuration not present in the codebase.
