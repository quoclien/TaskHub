# TaskHub Project Overview & Knowledge Base

*This document is the single source of truth for the TaskHub project. It serves as a living knowledge base, capturing our architecture, development patterns, and key learnings. It must be kept up-to-date.*

---

## 1. Core Principles & AI Collaboration Model

- **AI as the "Driver", Human as the "Navigator"**: We follow a pair-programming model. The user (Navigator) provides direction, defines requirements, and reviews all code. Cascade (Driver) implements the code, runs tests, and performs fixes.
- **Test-Driven Development (TDD)**: All features start with a failing test. The workflow is: Red (write a failing test) -> Green (write the minimum code to pass) -> Refactor (improve the code).
- **Incremental Validation**: Complex features are broken down into small, verifiable steps. We run health checks frequently to ensure stability.
- **Continuous Learning**: This document is our persistent memory. We update it with every new pattern, resolved issue, and architectural decision.

---

## 2. System Design & Architecture

### 2.1. Tech Stack

- **Frontend**: Next.js 15.3, React 19, TypeScript 5.6, Shadcn/ui, TanStack Query v5, Zod, @dnd-kit, NextAuth.js v5.
- **Backend**: ASP.NET Core 9, EF Core 9, PostgreSQL, SignalR, Serilog.
- **DevOps & Tools**: pnpm, Docker, GitHub Actions, ESLint, Prettier, Vitest, xUnit.

### 2.2. Monorepo & File Structure

- **Strategy**: We use a `pnpm` workspace monorepo to manage our `apps` and `packages`.
- **Next.js & TypeScript**: We **do not** use TypeScript Project References (`composite: true`). Instead, we use `transpilePackages` in `next.config.js` to compile workspace packages, ensuring Hot Module Replacement (HMR) works correctly.
- **Structure Diagram**:
  ```
  taskhub/
  ├── apps/
  │   ├── web/           # Next.js frontend
  │   └── api/           # ASP.NET Core backend
  ├── packages/
  │   ├── shared/        # Shared TS types (Zod schemas)
  │   └── ui/            # Shared React components (future)
  ├── .github/           # CI/CD workflows
  └── docker-compose.yml # Local development
  ```

### 2.3. Data Model

- **User**: Authentication and profile information.
- **Board**: Project workspace with members.
- **List**: Columns within boards (e.g., "To Do", "In Progress").
- **Card**: Tasks with metadata stored in PostgreSQL JSONB.
- **Comment**: Threaded discussions on cards.

### 2.4. Authentication & Real-time

- **Authentication Flow**: `NextAuth.js` handles the OAuth flow and creates a JWT, which is stored in a secure, HTTP-only cookie. A custom ASP.NET Core middleware validates the JWT on every request and SignalR connection.
- **Real-time Collaboration**: `SignalR` hubs manage group-based messaging for optimistic updates, user presence, and typing indicators. We use a "last-writer-wins" concurrency strategy.

### 2.5. API Design & Performance

- **API Design**: We use RESTful endpoints with OpenAPI documentation, consistent error handling, rate limiting, and health check endpoints.
- **Performance Strategy**: We focus on client-side caching with TanStack Query, EF Core query optimization, image optimization, and code splitting.
- **Performance Targets**: FCP < 1.5s, LCP < 2.5s, CLS < 0.1, API Response < 200ms (p95).

---

## 3. Key Architectural Decisions

- **Monorepo**: For simplified dependency management and code sharing.
- **PostgreSQL + JSONB**: Combines relational integrity with flexible document storage for card metadata.
- **Next.js App Router**: To leverage modern React features like Server Components.
- **SignalR**: For its deep integration within the ASP.NET ecosystem for real-time features.
- **TanStack Query**: For powerful server-state management, caching, and synchronization.

---

## 4. Development Guidelines & Conventions

### 4.1. Code Quality & Organization

- **Quality**: Enforce TypeScript `strict` mode, ESLint rules, and Prettier formatting via pre-commit hooks.
- **Organization**: Adhere to the Single Responsibility Principle, aim for < 200 lines per file, and use comments only for complex business logic.

### 4.2. Testing Strategy

- **TDD**: All new features must begin with a failing test.
- **Scope**: Unit tests for logic (Vitest/xUnit), component tests for UI (Vitest/Testing Library), and integration tests for API endpoints.
- **Maintenance**: Tests are a part of the feature. No PR is complete without passing and updated tests. Aim for high coverage on core logic.

### 4.3. Security

- **Validation**: Zod for frontend and API contract validation; FluentValidation for backend business logic.
- **Backend**: Prevent SQL injection with EF Core's parameterized queries.
- **Frontend**: Implement a strict Content Security Policy (CSP) to prevent XSS.

### 4.4. Conventions

- **File Naming**: `PascalCase.tsx` (Components), `camelCase.ts` (Hooks/Utils).
- **Commits**: `type(scope): description` (e.g., `feat(api): add card creation endpoint`).
- **API Endpoints**: RESTful (`/api/boards/{id}/cards`), use nouns, and be consistent.
- **Shared Types**: The `packages/shared` directory is the source of truth (Zod schemas). The .NET backend models must be manually kept in sync.

---

## 5. Knowledge Base: Resolved Issues & Error Patterns

*This section is a log of problems we've solved. Review this to avoid repeating past mistakes.*

### 5.1. Resolved Issues

| Problem | Solution & Context |
| :--- | :--- |
| **Vite/Vitest Version Mismatch** | **Solution**: Standardized all Vite-related dependencies in the root `package.json`. **Context**: Inconsistent versions caused cryptic build and test failures. Centralizing versions with `pnpm` workspaces resolved this. |
| *Add new issues here* | *...as they are resolved* |

### 5.2. Known Error Patterns & Fixes

| Error Message / Pattern | Common Cause & Fix |
| :--- | :--- |
| **`The type or namespace name '...' could not be found` in C#** | **Cause**: Missing `using` statement. **Fix**: Ensure `using Microsoft.AspNetCore.Mvc;` in controllers, `using Microsoft.EntityFrameworkCore;` in DbContext files, etc. |
| **Next.js Hydration Error** | **Cause**: Mismatch between server-rendered and client-rendered HTML. **Fix**: Use `useEffect` for client-side-only code or dynamic imports with `ssr: false`. |
| **EF Core Migration PK Error** | **Cause**: An entity is missing a primary key. **Fix**: Ensure every entity has a property like `public int Id { get; set; }`. |

---

## 6. Session Journal (Template)

*Use a temporary, git-ignored file (e.g., `session-log.md`) to document work-in-progress during a development session.*

```markdown
### Date: YYYY-MM-DD

**Goal**: [Briefly describe the main objective for the session]

**Key Steps & Learnings**:
- **Step 1: [Action taken]**
  - **Learning/Decision**: [What was learned? What decision was made?]

- **Step 2: [Action taken]**
  - **Error Encountered**: [Paste error message]
  - **Resolution**: [How was it fixed? Add to Knowledge Base if applicable.]

**Session Summary**:
- **Accomplishments**: [What was completed?]
- **New Patterns**: [Any new reusable patterns discovered?]
- **Next Steps**: [What's the plan for the next session?]
```