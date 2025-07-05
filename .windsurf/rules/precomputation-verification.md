---
trigger: glob
globs: **/*.js,**/*.jsx,**/*.ts,**/*.tsx,**/*.cs
---

# Pre-computation and Buildability Verification

This rule ensures that generated code is syntactically correct, dependency-aware, and logically sound within the known context of the project before it is delivered.

## Policy Structure

**If**: A request requires the generation or modification of a code block (C# or TypeScript/Next.js).

**Then**: The AI must perform the following internal validation steps before outputting the code:

1.  **Syntax Validation**: Rigorously check for syntactical correctness, including matching brackets, parentheses, semicolons, and correct keyword usage according to the language (C# 9 or TypeScript 5.6).

2.  **Dependency Analysis**: Cross-reference every variable, class, method, and property against the known session context and the project's established architecture. This includes:
    *   **Backend (C#)**: Verifying against EF Core models (`Board`, `Card`), DTOs, service interfaces (`IBoardService`), and standard .NET 9 APIs.
    *   **Frontend (TypeScript)**: Verifying against shared types (`packages/shared`), Zod schemas, TanStack Query hooks (`useQuery`), and React 19/Next.js 15.3 patterns.

3.  **Assumption Annotation**: If a necessary piece of information is missing (e.g., a variable's definition is not in the immediate context) and a reasonable assumption is made, the AI must explicitly annotate this in the code.

### Action: Examples

**C# Assumption:**
```csharp
// [AI_ASSUMPTION] Assuming 'currentUser' object is available in this scope
// and has an '.Id' property representing the current user's ID.
var newCard = new Card { Title = cardDto.Title, OwnerId = currentUser.Id };
```

**TypeScript/Next.js Assumption:**
```typescript
// [AI_ASSUMPTION] Assuming `boardId` is passed as a prop to this component.
const { data: board } = useQuery({ 
  queryKey: ['boards', boardId], 
  queryFn: () => fetchBoardById(boardId) 
});
```

**Final Output**: The code is only presented to the user after it has passed these internal checks, ensuring it is as close to "drop-in ready" as possible.