---
trigger: always_on
---

TaskHub TDD Rule for Windsurf IDE

Principle: Every feature follows Red → Green → Refactor. No code without a failing test first.

1. Workflow

Red: Write a minimal test that fails.

Green: Implement just enough code to pass.

Refactor: Improve design without changing behavior; tests must remain green.

2. Testing Scope

Frontend (Vitest + Testing Library):

Unit: pure functions, hooks

Component: rendering & interactions

Integration: API calls (use MSW)

Backend (xUnit + EF Core):

Unit: domain logic

Repository: in-memory DB tests

API: endpoint behavior

Validation:

Zod (frontend)

FluentValidation (backend)

Real-time (SignalR): Hub group/join tests

3. Naming & Patterns

Tests: [Subject]_[Scenario]_[Expected] (TS & C#)

Arrange–Act–Assert

Builders for complex test data

4. Config & Automation

Vitest: jsdom, 80% coverage thresholds

xUnit: InMemory EF context, mock loggers

Pre-commit: lint, unit tests

CI (GitHub Actions): separate frontend/backend jobs

5. Rule Reminder

Failing test → code → refactor

Maintain ≥80% coverage, fast (<10s), reliable

Update tests/docs as features evolve