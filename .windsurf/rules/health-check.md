---
trigger: model_decision
description: Before any code generation or after completing changes, run this sequence to validate project health.
---

# TaskHub Project Health Check

This rule defines a comprehensive, multi-stage health check to ensure project integrity, quality, and security. It should be executed from the monorepo root.

## EXECUTION TRIGGERS
- **BEFORE** any code generation task
- **AFTER** completing code changes
- **As a required check in the CI/CD pipeline**

## HEALTH CHECK SEQUENCE

### 1. Global Dependency & Environment Setup
*Goal: Ensure a consistent and clean environment for all subsequent checks.*
- **Install All Dependencies**:
  - `run_command`: `pnpm install --frozen-lockfile`
  - `cwd`: `d:\\Code\\TaskHub`
- **Clean Backend Artifacts**:
  - `run_command`: `dotnet clean`
  - `cwd`: `d:\\Code\\TaskHub\\apps\\api`

### 2. Code Quality & Style Enforcement
*Goal: Verify that all code adheres to the project's defined style and formatting standards.*
- **Backend Code Style Check**:
  - `run_command`: `dotnet format --verify-no-changes`
  - `cwd`: `d:\\Code\\TaskHub\\apps\\api`
  - *Note: This command checks the project in the specified directory.*
- **Frontend Linting & Style Check**:
  - `run_command`: `pnpm -r run lint -- --max-warnings 0`
  - `cwd`: `d:\\Code\\TaskHub`

### 3. Build, Compilation, and Type Safety
*Goal: Ensure the entire application compiles successfully and all types are consistent.*
- **Full-Stack Build**:
  - `run_command`: `pnpm -r run build`
  - `cwd`: `d:\\Code\\TaskHub`
- **Backend Build (for verification)**:
  - `run_command`: `dotnet build --no-incremental --configuration Release`
  - `cwd`: `d:\\Code\\TaskHub\\apps\\api`

### 4. Automated Testing
*Goal: Confirm that all existing functionality works as expected through unit and integration tests.*
- **Backend Tests**:
  - `run_command`: `dotnet test --no-build --configuration Release --logger "console;verbosity=minimal"`
  - `cwd`: `d:\\Code\\TaskHub\\apps\\api`
- **Frontend & Shared Tests**:
  - `run_command`: `pnpm -r run test:run`
  - `cwd`: `d:\\Code\\TaskHub`

### 5. Security & Dependency Health
*Goal: Proactively identify and report known vulnerabilities and outdated packages.*
- **.NET Vulnerability Scan**:
  - `run_command`: `dotnet list package --vulnerable`
  - `cwd`: `d:\\Code\\TaskHub\\apps\\api`
- **.NET Outdated Packages Check**:
  - `run_command`: `dotnet list package --outdated`
  - `cwd`: `d:\\Code\\TaskHub\\apps\\api`
- **Frontend Vulnerability Scan**:
  - `run_command`: `pnpm audit`
  - `cwd`: `d:\\Code\\TaskHub`

### 6. Database Schema Integrity Check
*Goal: Prevent runtime errors by ensuring the connected database schema is up-to-date with the code.*
- **Check for Pending EF Core Migrations**:
  - `run_command`: `dotnet ef migrations list`
  - `cwd`: `d:\\Code\\TaskHub\\apps\\api`
  - *Note: This command lists all migrations. If any migrations are listed that haven't been applied to the database, they should be investigated and applied.*

## FAILURE RESPONSE PROTOCOL

On ANY failure:
1.  **STOP** the current task immediately.
2.  **LOG** the exact command that failed and its full error output.
3.  **ANALYZE** the error and create a plan to resolve it.
4.  **RE-RUN** the health check after applying the fix to ensure the issue is resolved.

**Success is defined as all commands returning exit code 0.**
