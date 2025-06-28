## Technology Stack

### Frontend
* **Next.js 15.3** with App Router and React 19
* **TypeScript 5.6** (strict mode)
* **Shadcn/ui** with Tailwind CSS
* **TanStack Query v5** for server state
* **React Hook Form** with Zod validation
* **@dnd-kit/core** for drag-and-drop
* **NextAuth.js v5** for authentication

### Backend
* **ASP.NET Core 9** with minimal APIs
* **Entity Framework Core 9**
* **PostgreSQL** with JSONB support
* **SignalR** for real-time features
* **Serilog** for structured logging

### DevOps & Tools
* **Docker** containerization
* **GitHub Actions** for CI/CD
* **ESLint 9** and **Prettier**
* **xUnit**, **Jest**, **Playwright** for testing
* **Railway/Render** for deployment
* **PNPM** as the preferred package management

---
## File Structure
---

trello-clone/
├── apps/
│   ├── web/                     # Next.js frontend
│   │   ├── app/                 # App Router pages
│   │   ├── components/          # React components
│   │   ├── lib/                 # Utilities and configurations
│   │   └── types/               # TypeScript type definitions
│   └── api/                     # ASP.NET Core backend
│       ├── Controllers/         # API controllers
│       ├── Models/              # Entity models
│       ├── Hubs/                # SignalR hubs
│       └── Services/            # Business logic
├── packages/
│   └── shared/                  # Shared TypeScript types
├── docker-compose.yml           # Local development environment
├── .github/workflows/           # CI/CD pipelines
└── docs/                        # Project documentation


---
## System Design and Architecture
---

### Data Model
* **User**: Authentication and profile information
* **Board**: Project workspace with members and permissions
* **List**: Columns within boards (e.g., "To Do", "In Progress")
* **Card**: Tasks with metadata stored in PostgreSQL JSONB
* **Comment**: Threaded discussions on cards

### Authentication Architecture
* **NextAuth.js** with Google OAuth provider
* **JWT tokens** in HTTP-only cookies
* **Role-based authorization** (Owner, Member, Viewer)
* **Session management** across frontend and backend

### Real-time Collaboration
* **SignalR hubs** with group-based messaging
* **Optimistic updates** with conflict resolution
* **User presence** and typing indicators
* **Last-writer-wins** concurrency strategy

### API Design
* **RESTful endpoints** with OpenAPI documentation
* **Consistent error handling** and validation
* **Rate limiting** and security headers
* **Health check endpoints** for monitoring

### Performance Strategy
* **Client-side caching** with TanStack Query
* **Database query optimization** with EF Core
* **Image optimization** and CDN integration
* **Code splitting** and lazy loading

---
## Key Architectural Decisions
---

* **Monorepo Structure**: Single repository for better code organization and recruiter accessibility.
* **PostgreSQL over MongoDB**: Relational data model suits task management better, with JSONB for flexibility.
* **Next.js App Router**: Utilizes the latest React patterns with Server Components for SEO and performance.
* **SignalR over WebSockets**: Provides an integrated real-time solution within the .NET ecosystem.
* **TanStack Query over Redux**: Modern server state management with built-in caching and synchronization.
* **Shadcn/ui Components**: Offers production-ready UI components with accessibility built-in.

---
## Development Guidelines
---

### Code Quality
* **TypeScript strict mode** with exact optional properties
* **ESLint rules** for consistent code style
* **Prettier** for automatic formatting
* **Pre-commit hooks** with Husky and lint-staged

### Testing Strategy
* **Unit tests** for business logic and utilities
* **Integration tests** for API endpoints
* **Component tests** for React components
* **E2E tests** for critical user journeys

### Security Considerations
* **Input validation** with Zod schemas
* **SQL injection prevention** with EF Core
* **XSS protection** with Content Security Policy
* **CORS configuration** for API access

### Performance Targets
* **First Contentful Paint**: < 1.5s
* **Largest Contentful Paint**: < 2.5s
* **Cumulative Layout Shift**: < 0.1
* **API response time**: < 200ms (95th percentile)

---
## Coding Conventions
---
### File Naming
* **React components**: `PascalCase.tsx` (e.g., `BoardCard.tsx`, `UserAvatar.tsx`)
* **Utilities and hooks**: `camelCase.ts` (e.g., `useAuth.ts`, `formatDate.ts`)
* **API controllers**: `PascalCase.cs` (e.g., `BoardsController.cs`)
* **Database models**: `PascalCase.cs` (e.g., `Board.cs`, `Card.cs`)
* **Test files**: `ComponentName.test.tsx` or `ClassName.test.cs`

### Variable Naming
* **Constants**: `SCREAMING_SNAKE_CASE` (e.g., `API_BASE_URL`, `MAX_CARDS_PER_LIST`)
* **Functions**: `camelCase` with verb prefixes (e.g., `getUserBoards`, `createCard`, `validateInput`)
* **React props**: `camelCase` with descriptive names (e.g., `isLoading`, `onCardMove`, `boardId`)
* **C# properties**: `PascalCase` (e.g., `CreatedAt`, `BoardId`, `IsArchived`)
* **Database fields**: `snake_case` in migrations (e.g., `created_at`, `board_id`)

### Component Structure
1. Imports (external, internal, types)
2. Types and interfaces
3. Component function with props destructuring
4. Custom hooks and state
5. Event handlers (handle prefix)
6. Effects and side effects
7. Render functions (render prefix)
8. Main JSX return

### API Endpoint Naming
* **RESTful conventions**: `/api/boards/{id}/lists/{listId}/cards`
* **Actions**: `/api/boards/{id}/members` (POST to add, DELETE to remove)
* **Bulk operations**: `/api/cards/batch-update`

### Commit Message Format
`type(scope): description`

**Examples**:
* `feat(auth): add Google OAuth integration`
* `fix(api): resolve card ordering bug`
* `docs(readme): update setup instructions`
* `test(boards): add integration tests`
* `refactor(ui): extract reusable modal component`

### Error Handling
* **Frontend**: Use **Error Boundaries** and consistent error states.
* **Backend**: Return structured error responses with **correlation IDs**.
* **Logging**: Include context with structured data (e.g., `userId`, `boardId`, `action`).

### Code Organization
* Max 200 lines per file
* Single responsibility principle
* Meaningful function names that describe behavior
* Comments for complex business logic only
* Type annotations for all public interfaces