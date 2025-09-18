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

- **Frontend**: 
  - Next.js 15.3.4 (App Router)
  - React 19 with Server Components
  - TypeScript 5.3.3 (strict mode)
  - TanStack Query v5.8.1
  - Zod 3.25.67 for schema validation
  - Radix UI primitives
  - Tailwind CSS with shadcn/ui
  - Zustand for client state management

- **Backend**: 
  - .NET 8.0 Web API
  - Entity Framework Core 8.0
  - PostgreSQL 15+ with Npgsql 9.0.3
  - ASP.NET Core Identity with JWT
  - SignalR for real-time features
  - Serilog for structured logging
  - FluentValidation 11.3.0
  - AutoMapper 12.0.1

- **Testing**:
  - Vitest 1.3.1 (unit & integration)
  - React Testing Library
  - MSW 2.10.2 (API mocking)
  - xUnit for .NET unit tests

- **DevOps & Tools**:
  - pnpm 9.15.4 (workspaces)
  - ESLint 9.30.0 + Prettier 3.0.0
  - TypeScript 5.3.3
  - Swagger/OpenAPI 6.5.0
  - GitHub Actions for CI/CD
  - Docker for containerization

### 2.2. Monorepo & File Structure

- **Monorepo Strategy**:
  - pnpm workspaces for package management
  - Shared code in `packages/` directory
  - Independent versioning for each app/package
  - Shared tooling and configurations

- **TypeScript Setup**:
  - Strict mode enabled across all projects
  - Path aliases for cleaner imports
  - Shared `tsconfig.base.json` with common compiler options
  - Next.js uses `transpilePackages` for workspace packages

- **Project Structure**:
  ```
  taskhub/
  ├── apps/
  │   ├── web/                  # Next.js 15.3.4 frontend
  │   │   ├── src/
  │   │   │   ├── app/          # App Router
  │   │   │   ├── components/   # Shared components
  │   │   │   ├── lib/         # Utilities and hooks
  │   │   │   └── styles/      # Global styles
  │   │   └── public/          # Static assets
  │   │
  │   └── api/                  # .NET 8.0 Web API
  │       ├── Controllers/      # API endpoints
  │       ├── Data/            # DbContext and configurations
  │       ├── Models/          # Domain models
  │       ├── Services/        # Business logic
  │       └── appsettings.json # Configuration
  │
  ├── packages/
  │   └── shared/              # Shared TypeScript types and utilities
  │       ├── src/
  │       │   ├── types/       # Shared TypeScript types
  │       │   ├── schemas/     # Zod schemas
  │       │   └── utils/       # Shared utilities
  │       └── package.json     # @taskhub/shared package
  │
  ├── .github/                # GitHub Actions workflows
  ├── .husky/                # Git hooks
  ├── .vscode/               # Workspace settings
  ├── docker-compose.yml     # Local development
  ├── package.json           # Root package.json
  └── README.md              # Project documentation
  ```

- **Key Configuration Files**:
  - `package.json`: Root package with workspace configuration
  - `pnpm-workspace.yaml`: Defines workspace packages
  - `.eslintrc.js`: Shared ESLint configuration
  - `.prettierrc`: Code formatting rules
  - `tsconfig.base.json`: Base TypeScript configuration

### 2.3. Next.js App Router Conventions

- **Routing Structure**:
  ```
  src/app/
  ├── (auth)/                  # Authentication layout
  │   ├── sign-in/            # Sign in page
  │   └── sign-up/            # Sign up page
  │
  ├── (platform)/             # Authenticated app layout
  │   ├── boards/             # Boards list
  │   │   └── [boardId]/      # Single board view
  │   │       ├── page.tsx    # Board page
  │   │       └── _components/ # Board-specific components
  │   └── settings/           # User settings
  │
  ├── _components/            # App-wide components (not routes)
  ├── _lib/                   # Shared utilities and hooks
  ├── api/                    # API routes
  ├── favicon.ico
  ├── globals.css
  ├── layout.tsx              # Root layout
  └── page.tsx                # Home page
  ```

- **Key Conventions**:
  - **Layouts**: Use `layout.tsx` for shared UI across routes
  - **Loading States**: `loading.tsx` for Suspense boundaries
  - **Error Handling**: `error.tsx` for error boundaries
  - **Not Found**: `not-found.tsx` for 404 pages
  - **Route Groups**: `(folderName)` for logical grouping
  - **Dynamic Segments**: `[param]` for dynamic routes
  - **Private Folders**: `_prefix` for non-route directories
  - **Loading States**: Built-in Suspense with `loading.tsx`
  - **Error Boundaries**: Automatic with `error.tsx`

- **Data Fetching**:
  - **Server Components**: Default for data fetching
  - **Client Components**: Marked with `'use client'`
  - **Data Fetching**: `fetch` with Next.js caching
  - **Mutations**: Server Actions or API routes

- **Performance Features**:
  - Automatic Code Splitting
  - Image Optimization
  - Font Optimization
  - Script Optimization
  - Route Prefetching

### 2.4. Data Model

- **User**: Authentication and profile information.
- **Board**: Project workspace with members.
- **List**: Columns within boards (e.g., "To Do", "In Progress").
- **Card**: Tasks with metadata stored in PostgreSQL JSONB.
- **Comment**: Threaded discussions on cards.

### 2.5. Authentication & Real-time

- **Authentication Flow**: `NextAuth.js` handles the OAuth flow and creates a JWT, which is stored in a secure, HTTP-only cookie. A custom ASP.NET Core middleware validates the JWT on every request and SignalR connection.
- **Real-time Collaboration**: `SignalR` hubs manage group-based messaging for optimistic updates, user presence, and typing indicators. We use a "last-writer-wins" concurrency strategy.

### 2.6. API Design & Performance

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

- **Testing Pyramid**:
  - **Unit Tests (70%)**: Test individual functions and components in isolation
  - **Integration Tests (20%)**: Test component interactions and API integrations
  - **E2E Tests (10%)**: Test critical user flows

- **Frontend Testing**:
  - **Unit Tests**: Vitest for pure functions and hooks
  - **Component Tests**: React Testing Library + Vitest
  - **Mocking**: MSW for API mocking
  - **Coverage**: Aim for 80%+ coverage on business logic

- **Backend Testing**:
  - **Unit Tests**: xUnit for business logic
  - **Integration Tests**: Test API endpoints with TestServer
  - **Database Tests**: Test database operations with SQLite in-memory

- **Test Organization**:
  - Co-locate test files with source files (e.g., `Component.test.tsx`)
  - Use `__tests__` directories for test utilities
  - Follow the "Arrange-Act-Assert" pattern

- **Test Data Management**:
  - Use factories for test data generation
  - Clean up test data after each test
  - Use snapshots sparingly, prefer explicit assertions

### 4.3. Security

- **Authentication & Authorization**:
  - JWT-based authentication
  - Role-based access control (RBAC)
  - Secure cookie settings
  - CSRF protection

- **Input Validation**:
  - Frontend: Zod schema validation
  - Backend: FluentValidation + ModelState validation
  - API: Strongly-typed API contracts

- **Security Headers**:
  - Content Security Policy (CSP)
  - X-Content-Type-Options
  - X-Frame-Options
  - X-XSS-Protection
  - Strict-Transport-Security

- **Rate Limiting**:
  - Global rate limiting
  - Endpoint-specific rate limiting
  - IP-based throttling

- **Data Protection**:
  - Parameterized queries with EF Core
  - Sensitive data encryption
  - Secure password hashing (BCrypt)
  - Secure storage of secrets

- **API Security**:
  - HTTPS required
  - API versioning
  - Request validation
  - Response sanitization
  - CORS policy configuration

### 4.4. Conventions

- **File Naming**: `PascalCase.tsx` (Components), `camelCase.ts` (Hooks/Utils).
- **Commits**: `type(scope): description` (e.g., `feat(api): add card creation endpoint`).
- **API Endpoints**: RESTful (`/api/boards/{id}/cards`), use nouns, and be consistent.
- **Shared Types**: The `packages/shared` directory is the source of truth (Zod schemas). The .NET backend models must be manually kept in sync.

---

## 5. Knowledge Base: Resolved Issues & Error Patterns

*This section documents common issues and their solutions to help with troubleshooting and knowledge sharing.*

### 5.1. Resolved Issues

| Problem | Solution & Context |
| :--- | :--- |
| **Vite/Vitest Version Mismatch** | **Solution**: Standardized Vite-related dependencies in root `package.json`. **Context**: Inconsistent versions caused build/test failures. Fixed by centralizing versions with pnpm workspaces. |
| **.NET 8.0 Migration Issues** | **Solution**: Updated all NuGet packages to .NET 8.0 compatible versions. **Context**: Some packages had breaking changes in .NET 8.0 that required code updates. |
| **MSW Interference with Vitest** | **Solution**: Configured MSW to only run in test environment. **Context**: MSW was causing issues during development server startup. |

### 5.2. Common Error Patterns & Fixes

| Error Message / Pattern | Common Cause & Fix |
| :--- | :--- |
| **`"Invalid hook call" in React** | **Cause**: Hooks called outside of a component or multiple React instances. **Fix**: Ensure single React version and proper component structure. |
| **`Npgsql.PostgresException: 42P01: relation "__EFMigrationsHistory" does not exist`** | **Cause**: Database migrations not applied. **Fix**: Run `dotnet ef database update`. |
| **`TypeError: fetch failed` in Next.js** | **Cause**: Missing or incorrect API route handler. **Fix**: Ensure route handler exists and exports proper HTTP methods. |
| **`Error: No QueryClient set`** | **Cause**: Missing QueryClientProvider. **Fix**: Wrap app with QueryProvider in root layout. |
| **`CS1061: 'Type' does not contain a definition for 'Method'`** | **Cause**: Missing using directive or incorrect type reference. **Fix**: Add proper using statements or check for typos. |

### 5.3. Performance Optimization Tips

- **Frontend**:
  - Use `React.memo` for expensive components
  - Implement code splitting with dynamic imports
  - Optimize images with Next.js Image component
  - Use `useCallback` and `useMemo` appropriately

- **Backend**:
  - Use `AsNoTracking()` for read-only queries
  - Implement pagination for large datasets
  - Use compiled queries for frequently executed queries
  - Enable query caching where appropriate

### 5.4. Development Workflow Tips

- **Hot Reloading**:
  - Frontend: `pnpm dev` for Next.js development server
  - Backend: `dotnet watch` for .NET hot reload

- **Database Migrations**:
  - Create: `dotnet ef migrations add <name>`
  - Apply: `dotnet ef database update`
  - Remove: `dotnet ef migrations remove`

- **Testing**:
  - Run all tests: `pnpm test`
  - Run specific test file: `pnpm test path/to/test.tsx`
  - Update snapshots: `pnpm test -u`
  - Coverage report: `pnpm test:coverage`

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