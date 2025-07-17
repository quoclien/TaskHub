# TaskHub Project Overview

## Project Purpose
TaskHub is a modern Trello-like task management application with real-time collaboration features. Built as a full-stack monorepo demonstrating clean architecture principles.

## Technology Stack
**Frontend**: Next.js 15.3, React 19, TypeScript 5.6, Shadcn/ui, TanStack Query v5, NextAuth.js v5
**Backend**: ASP.NET Core 9, EF Core 9, PostgreSQL, SignalR, Serilog
**DevOps**: pnpm workspaces, Docker, GitHub Actions, ESLint, Prettier, xUnit, Vitest

## Architecture
- Monorepo structure with pnpm workspaces
- apps/web (frontend), apps/api (backend), packages/shared (types)
- Repository pattern with dependency injection
- JWT authentication with NextAuth.js
- Real-time updates via SignalR