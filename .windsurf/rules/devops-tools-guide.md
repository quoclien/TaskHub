---
trigger: model_decision
description: When working with any of our DevOps and Tools tech stack, or when manually use
---

# TaskHub DevOps & Tools: Best Practices Guide

This document outlines the standards and workflows for our development operations, tooling, and CI/CD pipelines. Adherence to these guidelines is critical for maintaining a stable, efficient, and scalable development environment.

---

## 1. Monorepo & Dependency Management (PNPM)

Our monorepo is managed with PNPM workspaces, which is efficient for managing dependencies and running scripts across projects.

- **Workspace Configuration**: The root `pnpm-workspace.yaml` file must define the locations of our applications and shared packages (e.g., `apps/*`, `packages/*`).
- **Internal Dependencies**: When one workspace package (e.g., `web`) depends on another (e.g., `shared`), the dependency in `package.json` must use the `workspace:*` protocol (e.g., `"@taskhub/shared": "workspace:*"`). This ensures PNPM uses the local version.
- **Installing Dependencies**: Always run `pnpm install` from the root of the monorepo. This installs dependencies for all workspaces and correctly links internal packages. Never run `npm install` or `yarn`.
- **Running Scripts**: To run a script for a specific application, use the `--filter` flag (e.g., `pnpm --filter web dev`). To run a script across all workspaces, use the `-r` (recursive) flag (e.g., `pnpm -r build`).

---

## 2. Continuous Integration & Deployment (GitHub Actions)

Our CI/CD pipelines are defined in `.github/workflows`. They are responsible for building, testing, and deploying our applications.

- **Workflow Triggers**: Workflows should be triggered on pushes and pull requests to the `main` branch. Use `paths` filters to run jobs only when relevant code has changed. For example, the backend CI should only run if files in `apps/api/**` are modified.

  **Example (`ci-backend.yml`):**
  ```yaml
  on:
    push:
      branches: [ main ]
    pull_request:
      branches: [ main ]
      paths:
        - 'apps/api/**'
        - 'packages/shared/**'
  ```

- **Dependency Caching**: Use `pnpm/action-setup` in conjunction with `actions/setup-node` to install PNPM and cache dependencies. This dramatically speeds up CI runs.

  **Example (`ci-frontend.yml`):**
  ```yaml
  - uses: actions/checkout@v4
  - uses: pnpm/action-setup@v3
    with:
      version: 9
  - name: Set up Node.js
    uses: actions/setup-node@v4
    with:
      node-version: '20'
      cache: 'pnpm'
  - name: Install dependencies
    run: pnpm install
  ```

- **Separate Jobs for Frontend & Backend**: The CI workflow should have distinct jobs for the `web` and `api` applications to run tests in parallel and manage environment-specific setups (Node.js vs. .NET).
- **Deployment**: Use environment secrets (e.g., `RAILWAY_API_TOKEN`) to securely deploy to our hosting providers (Railway/Render) after all build and test steps pass on the `main` branch.

---

## 3. Local Development Environment (Docker)

We use Docker and `docker-compose.yml` to create a consistent and isolated local development environment.

- **Services**: The `docker-compose.yml` file should define three services:
  1. `db`: A `postgres` service for the database.
  2. `api`: The ASP.NET Core backend.
  3. `web`: The Next.js frontend.
- **Hot-Reloading**: Use Docker volumes to map the local source code into the containers (e.g., `./apps/web:/app/web`). This enables hot-reloading when you make changes to the code.
- **Dockerfile Strategy**:
  - For the `api`, use a multi-stage `Dockerfile` that first restores dependencies, then builds the application, and finally runs it.
  - For the `web`, the `Dockerfile` should install PNPM dependencies and then run the `pnpm dev` command.

---

## 4. Testing Strategy (Vitest & xUnit)

- **Unit & Integration Tests**: All new features must be accompanied by tests.
  - **Frontend (`web`)**: Use **Vitest** for unit and component tests.
  - **Backend (`api`)**: Use **xUnit** for unit and integration tests.
- **CI Execution**: In the GitHub Actions workflow, run tests as part of the build and validation process for both frontend and backend jobs. A failing test must fail the entire CI run.
    - `pnpm --filter web test:run` (for Vitest)
    - `dotnet test` (for xUnit)
- **End-to-End (E2E) Tests**: **Playwright** tests should be in a separate workflow that can be run manually or on a schedule, as they are longer-running.

---

## 5. Code Quality & Formatting (ESLint & Prettier)

- **Linting & Formatting**: ESLint and Prettier are configured at the root of the monorepo.
- **Pre-commit Hooks**: Use **Husky** and **lint-staged** to automatically run ESLint and Prettier on staged files before they are committed. This ensures all code pushed to the repository adheres to our style guides.
- **CI Check**: The CI pipeline must include a step to run `pnpm lint` and `pnpm format:check`. A violation should fail the build.

---

## 6. Monitoring & Observability

- **Logging**: Use **Serilog** in the backend for structured logging. Logs should be exported to a centralized logging service (e.g., Seq, Datadog, or ELK stack) in production.
- **Error Tracking**: Integrate **Sentry** or a similar service to track and monitor errors in real-time.
- **Performance Monitoring**: Use **Application Insights** or **Datadog** to monitor application performance and set up alerts for critical issues.

---

## 7. Security Best Practices

- **Dependency Scanning**: Use **Dependabot** or **Renovate** to keep dependencies up-to-date and scan for vulnerabilities.
- **Secrets Management**: Store sensitive information (e.g., API keys, database credentials) in environment variables or a secrets management service (e.g., Azure Key Vault, AWS Secrets Manager). Never commit secrets to the repository.
- **Code Scanning**: Use **GitHub CodeQL** or **SonarQube** to perform static code analysis and identify security vulnerabilities.

---

## 8. Documentation

- **API Documentation**: Use **Swagger/OpenAPI** for the backend API documentation. Ensure all endpoints are documented with request/response schemas and examples.
- **Project Documentation**: Maintain a `README.md` in the root of the repository with setup instructions, development guidelines, and links to additional resources.
- **Architecture Decision Records (ADRs)**: Use ADRs to document significant architectural decisions and the rationale behind them. Store these in a `docs/adr` directory.

---

## 9. Review & Compliance

- **Code Reviews**: All changes must be reviewed by at least one other team member before merging into the `main` branch. Use pull requests with a clear description of the changes and any related issues.
- **Compliance**: Ensure the project complies with relevant regulations (e.g., GDPR, HIPAA) by conducting regular security audits and implementing necessary controls.

---

## 10. Continuous Improvement

- **Retrospectives**: Hold regular retrospectives to reflect on what went well and what could be improved in the development and deployment process.
- **Stay Updated**: Keep up with the latest tools, libraries, and best practices. Regularly review and update the technology stack as needed.

---

By following these guidelines, we ensure a consistent, efficient, and secure development workflow for the TaskHub project.