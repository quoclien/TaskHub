---
trigger: glob
description: When working with any of our defined Frontend tech stack, or when writing anything related to frontend technology and Next.js
globs: **/*.js,**/*.jsx,**/*.ts,**/*.tsx
---

# TaskHub Frontend: Best Practices & Style Guide

This document outlines the architectural patterns, coding conventions, and best practices for the Next.js frontend of TaskHub. Adherence to these guidelines is mandatory to ensure code quality, maintainability, and consistency.

## 1. Core Principles

* **Server Components First**: Default to using React Server Components (RSCs) for performance. Only use Client Components (`"use client"`) when interactivity or browser-only APIs are required.
* **Colocation**: Keep components, styles, tests, and related files together within feature-based folders to improve modularity and discoverability.
* **Single Responsibility**: Each component should have a single, well-defined purpose. Decompose large components into smaller, more manageable ones.
* **TypeScript Strict Mode**: Leverage TypeScript's full power to catch errors at compile time. Use specific types over `any`.

## 2. Project Structure & File Naming

We follow the standard Next.js App Router structure within the `apps/web` directory.

* `app/`: Contains all routes, layouts, and pages.
    * **Route Groups (`(group)`)**: Use route groups to organize sections of the app (e.g., `(auth)`, `(main)`) without affecting the URL path. This is ideal for applying different layouts to different sections.
    * **Private Folders `_folder`**: Use private folders for colocating internal UI components, hooks, and utilities that are not part of the routing system.
* `components/`:
    * `ui/`: Contains all `shadcn/ui` components, which are un-styled and can be modified.
    * `shared/`: Reusable, application-agnostic components (e.g., `Avatar`, `Logo`).
    * `feature/`: Components specific to a feature, often colocated within a private `_components` folder inside a route.
* `lib/`: Utility functions, constants, and third-party library configurations (e.g., `utils.ts`, `cn.ts`).
* `hooks/`: Custom React hooks (e.g., `useDebounce.ts`).
* **File Naming Conventions**:
    * **React Components**: `PascalCase.tsx` (e.g., `Board.tsx`, `Card.tsx`)
    * **Pages & Layouts**: `page.tsx`, `layout.tsx`, `loading.tsx`, `error.tsx` (lowercase)
    * **Hooks & Utilities**: `camelCase.ts` (e.g., `useBoardStore.ts`, `formatDate.ts`)
    * **Test Files**: `*.test.tsx` or `*.test.ts` (e.g., `Board.test.tsx`)

## 3. Data Fetching & State Management

### Data Fetching Strategy

* **Server Components**: Perform initial, read-only data fetching directly in Server Components using `async/await`. This leverages server-side rendering for faster page loads.
* **Parallel Fetching**: When a component needs data from multiple sources, initiate all requests in parallel using `Promise.all` to minimize waterfalls.
* **Client Components (TanStack Query)**: Use TanStack Query (`useQuery`) for all client-side data fetching, caching, and synchronization. This is essential for:
    * Data that changes based on user interaction (sorting, filtering).
    * Automatic refetching and background updates.
    * Optimistic updates for a faster perceived performance.
* **Mutations**: Use `useMutation` from TanStack Query for all create, update, and delete operations.

### State Management

* **Server State**: Managed exclusively by TanStack Query. It is the single source of truth for any data that comes from our API.
* **UI State**: Managed by React hooks (`useState`, `useReducer`). Keep UI state as close as possible to the components that use it.
* **Global UI State**: For state that needs to be shared across many components (e.g., theme, mobile sidebar open/closed), use Zustand. It's lightweight and avoids the need for context providers.

### TanStack Query Best Practices

* **Query Keys**: Use descriptive, serializable array keys. A common pattern is `[scope, { id, ...filters }]`. Example: `['boards', { orgId: '123' }]`.
* **Query Functions**: Abstract fetching logic into reusable functions (e.g., in an `api/` or `services/` directory).
* **`staleTime` vs. `cacheTime`**:
    * Set a `staleTime` (e.g., 5 minutes) to prevent unnecessary refetches of data that doesn't change often.
    * `cacheTime` (defaults to 5 minutes) determines how long inactive query data is kept in memory.

## 4. Forms & Validation

* **React Hook Form**: Use for all forms to optimize performance by minimizing re-renders.
* **Zod**: Define all data schemas and validation rules using Zod. This provides compile-time type safety and runtime validation from a single source of truth.
* **`zodResolver`**: Connect your Zod schema to React Hook Form using `@hookform/resolvers/zod`.

### Example Pattern

```typescript
// in lib/validators/card.ts
import { z } from 'zod';

export const CardSchema = z.object({
  title: z.string().min(1, "Title is required."),
  description: z.string().optional(),
});

// in components/forms/CreateCardForm.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { CardSchema } from '@/lib/validators/card';

type CardFormData = z.infer<typeof CardSchema>;

export function CreateCardForm() {
  const { register, handleSubmit, formState: { errors } } = useForm<CardFormData>({
    resolver: zodResolver(CardSchema),
  });

  const onSubmit = (data: CardFormData) => {
    // ... call mutation
  };

  // ... JSX form
}
```
## 5. UI & Styling with Shadcn/ui and Tailwind CSS

* **Composition over Configuration**: Build complex UI by composing smaller `shadcn/ui` components.
* **Styling**: Use Tailwind CSS utility classes directly in your JSX. Avoid writing custom CSS files whenever possible.
* **`cn` Utility**: Use the `cn` utility from `lib/utils.ts` to conditionally apply classes and merge Tailwind styles from props.
* **Theming**: Customize the application's look and feel by modifying the CSS variables defined in `app/globals.css`. Do not directly edit the component files in `components/ui`.

## 6. Component Design

Follow this structure for all React components to ensure consistency:

1.  **Imports**: Grouped as external, internal, and then types.
2.  **Type Definitions**: Interfaces or types for component props.
3.  **Component Function**:
    * Destructure props.
    * Initialize hooks (`useState`, `useQuery`, custom hooks).
    * Define event handlers (`handle...`).
    * Define effects (`useEffect`).
4.  **JSX Return**: Keep the return statement clean. Extract complex conditional rendering into separate functions or components if necessary.