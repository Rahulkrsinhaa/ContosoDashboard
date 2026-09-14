<!--
Sync Impact Report
- Version change: 0.0.0 -> 1.0.0
- Modified principles: N/A (new constitution baseline)
- Added sections: Core Principles, Training Constraints, Development Workflow, Governance
- Removed sections: none
- Follow-up TODOs: none
-->

# ContosoDashboard Constitution

## Core Principles

### I. Security-First for Training
The application must treat authentication, authorization, and user isolation as non-negotiable requirements. All protected pages, service methods, and data access paths must verify the current user and role before exposing information or actions. The training environment may use mock authentication, but it must still enforce claims, authorization checks, and direct-object validation as if it were production-sensitive code.

This principle exists because the project is explicitly designed for security training. Students must see the same failure modes and guardrails that production systems require, while keeping the demo self-contained and offline.

### II. Offline-First, Cloud-Ready Architecture
The project must remain runnable without cloud services and must keep infrastructure concerns behind abstractions. Business logic, domain rules, and UI behavior must not depend on a specific database provider, file storage medium, or external identity platform. Where infrastructure choices differ between training and production, the codebase must make the abstraction explicit and replaceable.

This principle preserves the training objective, reduces setup friction, and demonstrates a migration path to Azure-native services without rewriting core application logic.

### III. Testable and Incremental Delivery
Every change must be scoped, reviewable, and verifiable. Functional work must be broken into small, testable increments; new rules must be validated with the smallest meaningful tests around security, access control, or workflow behavior. When a bug is discovered, the fix must include a regression check that protects the observed behavior.

This principle prevents accidental scope creep and ensures the project remains understandable for learners and instructors during progressive SDD exercises.

### IV. User-Scoped Data Integrity
Data access must respect the current user, their team scope, and project membership. Tasks, projects, and notifications must never be exposed or mutated outside the authorized boundary unless the logic explicitly allows it. Authorization checks are mandatory in both UI flows and service-layer operations.

This principle prevents IDOR-style issues and teaches the expectation that user context is part of the data contract, not an optional validation step.

### V. Simplicity, Clarity, and Maintainability
The codebase must favor clear naming, explicit relationships, small service boundaries, and direct intent over clever abstractions. Features must be implemented in ways that are easy to explain, review, and extend within the training environment. Complexity must be justified with a clear business need or a teaching objective.

This principle keeps the repository approachable for training while preserving disciplined engineering habits that scale beyond the initial demo.

## Training Constraints

ContosoDashboard is a training-only application and must remain aligned with that purpose. The project may use local development defaults, mock identity flows, and simplified security implementations, but it must not claim production readiness or imply a supported deployment profile.

- Use ASP.NET Core 8.0 with Blazor Server and local tooling as the default training stack.
- Keep mock authentication and authorization explicit, documented, and limited to training scenarios.
- Prefer local database and file-system implementations for learning, while preserving abstractions for future migration.
- Do not introduce production-only assumptions, credentials, or cloud dependencies without clearly labeling them as non-training work.
- Preserve security and access-control examples as part of the intentionally educational architecture.

## Development Workflow

All work on this repository must follow a disciplined delivery process so that the application remains coherent and instructional.

- New features or fixes must be introduced as small, reviewable increments with a clear learning objective.
- Authentication, authorization, and data-scoping changes require explicit validation before merge.
- Changes that alter access patterns, data exposure, or service contracts must be reviewed for compliance with the security principles in this constitution.
- Documentation, examples, and comments must reflect the actual behavior of the code, especially for training scenarios that intentionally mimic production constraints without claiming full production parity.
- Pull requests must confirm that the changed behavior is consistent with the project’s learning goals and does not silently weaken security posture.

## Governance

This constitution governs the repository’s engineering standards, training intent, and project-level decision-making. It supersedes informal practices that conflict with these rules and must be treated as the reference baseline for future work.

Amendments require a documented rationale, a version bump under the policy below, and a review that confirms the change preserves the training mission and security discipline. Changes that materially alter the architecture, authorization model, or learning assumptions must be explicitly called out in the amendment record.

The project follows semantic versioning for governance updates:
- MAJOR: backward-incompatible or materially redefining governance changes
- MINOR: new principle or section added, or a substantive expansion of existing governance
- PATCH: wording, clarification, or non-semantic refinements

Compliance review is expected for any change affecting access control, business rules, identity flows, or architecture boundaries. If a change appears inconsistent with the Constitution, the team must either revise the work or document a justified exception before approval.

**Version**: 1.0.0 | **Ratified**: 2026-09-14 | **Last Amended**: 2026-09-14
