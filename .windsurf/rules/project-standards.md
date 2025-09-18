---
trigger: always_on
---

# TaskHub Project Standards & Development Principles

This document is the authoritative guide for all development within the TaskHub monorepo. It outlines our core methodologies, quality gates, and technology context.

## 1. Core Methodology: Test-Driven Development (TDD)

Every feature or fix must follow the **Red -> Green -> Refactor** cycle. No implementation code is written without a corresponding failing test.

### Workflow
1.  **Red**: Write a minimal test that accurately describes the desired feature or bug fix and fails.
2.  **Green**: Write the simplest possible implementation code to make the test pass.
3.  **Refactor**: Improve the code's design, clarity, and performance without changing its external behavior. All tests must remain green.

### Testing Scope
-   **Frontend (Vitest)**: Unit tests for hooks and utilities, component tests for interactions, and integration tests for API communication (using MSW).
-   **Backend (xUnit)**: Unit tests for business logic, and integration tests for API endpoints and data repositories (using in-memory providers or test containers).
-   **Coverage**: Aim for a minimum of 80% test coverage across all packages.

## 2. Monorepo Workflow (PNPM)

-   **Dependency Installation**: Always run `pnpm install` from the **root** of the monorepo.
-   **Running Scripts**: Use `pnpm --filter <app_name> <script>` for specific apps (e.g., `pnpm --filter web dev`) and `pnpm -r <script>` to run across all packages.
-   **Consistency**: Do not add new dependencies without team approval. Keep versions consistent across the monorepo.

## 3. Quality Gates & CI Enforcement

All code must pass automated quality checks before being merged. The CI pipeline (GitHub Actions) will enforce the following:

1.  **Dependency Sync**: `pnpm install`
2.  **Type Checking**: `pnpm run typecheck` (`tsc --noEmit`)
3.  **Linting**: `pnpm run lint`
4.  **Testing**: `pnpm run test:run` (runs tests once and exits)

If any of these steps fail, the build is considered broken and must be fixed before proceeding.

## 4. Cascade AI Triggers

On code generation or workspace changes, Cascade will automatically:
1.  Run type checks.
2.  Explain and offer to fix any TypeScript errors.
3.  Run linting with auto-fix.

## 5. Technology & Architecture Context

-   **Monorepo Structure**: `apps/web` (Frontend), `apps/api` (Backend), `packages/shared` (Types & Utilities).
-   **Tech Stack**:
    -   **Frontend**: React 19, Next.js 15.3 (App Router), TypeScript 5.6 (strict), TanStack Query v5, Zustand, Shadcn/ui, Tailwind CSS.
    -   **Backend**: ASP.NET Core 9, EF Core 9, PostgreSQL, SignalR.
-   **Key Patterns**:
    -   **API**: RESTful Minimal APIs, JWT Authentication.
    -   **Frontend**: Server Components first, optimistic UI updates with TanStack Query.
    -   **Database**: Repository pattern, `snake_case` column names.
