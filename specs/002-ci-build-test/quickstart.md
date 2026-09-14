# Quickstart: CI Build and Test Workflow

## Prerequisites

- GitHub repository with Actions enabled.
- .NET SDK matching the `TargetFramework` in `ContosoDashboard/ContosoDashboard.csproj`.
- Repository checkout with the existing application and test projects.

## Local Validation

From the repository root, run:

```powershell
dotnet restore .\ContosoDashboard.Tests\ContosoDashboard.Tests.csproj
dotnet build .\ContosoDashboard\ContosoDashboard.csproj --no-restore
dotnet test .\ContosoDashboard.Tests\ContosoDashboard.Tests.csproj --no-restore
```

Expected result: restore completes, the application builds, and all discovered tests pass.

Validated locally on 2026-09-14 with .NET SDK 10.0.401: restore completed, the application built with three existing nullable warnings, and one test passed.

## GitHub Actions Validation

1. Push a commit to any branch.
2. Open or update a pull request targeting `main`.
3. Open the Actions tab and select `Build and Test`.
4. Confirm the restore, build, and test steps complete successfully.
5. For a deliberate failure, inspect the failed step’s log output to identify the command and error.

## Merge Governance

The constitution requires passing CI before merge. Repository maintainers must configure the workflow check as a required status check in branch protection settings for `main`; the workflow file alone reports status but cannot enforce repository settings.

## References

- Feature requirements: [spec.md](spec.md)
- Technical decisions: [research.md](research.md)
- Artifact relationships: [data-model.md](data-model.md)
