---
trigger: model_decision
description: Before any code generation or after completing changes, run this sequence to validate project health.
---

# TaskHub Project Health Check

This rule defines a sequence of commands to be executed from the monorepo root to ensure the integrity of the entire application.

## EXECUTION TRIGGERS
- **BEFORE** any code generation task
- **AFTER** completing code changes

## HEALTH CHECK SEQUENCE

1.  **Project Structure Validation**
    -   `run_command`: `ls -la apps/web apps/api packages/shared`
    -   `cwd`: `d:\\Code\\TaskHub`

2.  **Backend Health Check (ASP.NET Core)**
    -   `run_command`: `dotnet clean`
    -   `cwd`: `d:\\Code\\TaskHub\\apps\\api`
    -   `run_command`: `dotnet build --configuration Release --verbosity minimal`
    -   `cwd`: `d:\\Code\\TaskHub\\apps\\api`
    -   `run_command`: `dotnet test --configuration Release --logger \"console;verbosity=minimal\" --no-build`
    -   `cwd`: `d:\\Code\\TaskHub\\apps\\api`

3.  **Frontend Health Check (Next.js)**
    -   `run_command`: `pnpm install --frozen-lockfile --silent`
    -   `cwd`: `d:\\Code\\TaskHub\\apps\\web`
    -   `run_command`: `pnpm run type-check`
    -   `cwd`: `d:\\Code\\TaskHub\\apps\\web`
    -   `run_command`: `pnpm run lint --max-warnings 0`
    -   `cwd`: `d:\\Code\\TaskHub\\apps\\web`
    -   `run_command`: `pnpm run test --run --reporter=verbose`
    -   `cwd`: `d:\\Code\\TaskHub\\apps\\web`

4.  **Shared Types Validation**
    -   `run_command`: `pnpm run type-check`
    -   `cwd`: `d:\\Code\\TaskHub\\packages\\shared`
    -   `run_command`: `pnpm run build`
    -   `cwd`: `d:\\Code\\TaskHub\\packages\\shared`

## FAILURE RESPONSE PROTOCOL

On ANY failure:
1.  **STOP** the current task.
2.  **LOG** the exact error and the failing command.
3.  **CATEGORIZE** the error (Critical, High, Medium) and prioritize the fix.
4.  **UPDATE PLAN** to address the failure before proceeding.
5.  **RETRY** the health check after applying the fix.

**Success is defined as all commands returning exit code 0.**
