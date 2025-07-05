---
trigger: model_decision
description: When user provides feedback indicating faulty code generation, compilation errors, runtime exceptions, or pattern violations, or when manually triggered for pattern enforcement.
---

# TaskHub Error-Driven Learning & Pattern Enforcement System

**Establishes adaptive learning mechanisms for TaskHub-specific development patterns, ensuring progressive improvement and context-aware code generation throughout the development session.**

## Core Objectives

1. **Eliminate Pattern Repetition**: Once an error is identified and resolved, prevent recurrence within the active session
2. **TaskHub Context Awareness**: Apply project-specific patterns from the knowledge base
3. **Progressive Intelligence**: Build session-specific wisdom that compounds over time
4. **Proactive Application**: Apply learned patterns before errors occur

## Implementation Framework

### Phase 1: Error Classification & Root Cause Analysis

**When error feedback is received, perform structured analysis:**

1. **Error Categorization**
   - **Compilation Error**: TypeScript/C# syntax, missing imports, type mismatches
   - **Runtime Exception**: Async/await violations, null reference, database connection
   - **Pattern Violation**: TDD bypass, architecture deviation, security oversight
   - **TaskHub-Specific**: Zod schema mismatch, SignalR connection issues, TanStack Query key conflicts

2. **Root Cause Mapping**
   - Map error to specific TaskHub tech stack components
   - Identify violated principle from project knowledge base
   - Determine if error stems from missing context or incorrect assumption

### Phase 2: Pattern Creation & Validation

**Create enforceable corrective patterns:**

1. **Pattern Formulation Structure**
   ```
   PATTERN_ID: [Unique identifier for session tracking]
   TRIGGER: [Specific conditions that should invoke this pattern]
   RULE: [Exact corrective action to take]
   CONTEXT: [TaskHub-specific context or constraints]
   VALIDATION: [How to verify pattern application]
   ```

2. **TaskHub-Specific Pattern Templates**
   - **Async Database Operations**: "All EF Core operations must use async variants (`ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`) with proper `await` handling"
   - **Zod Schema Sync**: "Backend DTOs must match frontend Zod schemas in `packages/shared`. Verify type compatibility before implementation"
   - **TanStack Query Keys**: "Query keys must follow established structure: `[entity, action, ...params]` for consistency with existing caching strategy"
   - **SignalR Group Management**: "All SignalR operations must include proper error handling and group membership validation"

### Phase 3: Session Memory & Enforcement

**Maintain active session awareness:**

1. **Pattern Storage**
   - Store all learned patterns in session memory with timestamps
   - Cross-reference with TaskHub knowledge base for consistency
   - Maintain pattern hierarchy (critical → important → preferential)

2. **Proactive Application**
   - Before generating code, check applicable patterns
   - Apply patterns automatically with acknowledgment
   - Flag potential pattern conflicts before implementation

3. **Pattern Validation**
   - Validate pattern application against TaskHub conventions
   - Ensure compatibility with existing codebase patterns
   - Verify no regression in established functionality

### Phase 4: Continuous Learning Integration

**Integrate with Memory MCP and session continuity:**

1. **Memory MCP Integration**
   - Store critical patterns in Memory MCP for cross-session persistence
   - Flag patterns that should become permanent project rules
   - Maintain pattern effectiveness metrics

2. **Knowledge Base Updates**
   - Identify patterns that should be added to project knowledge base
   - Recommend updates to TaskHub conventions based on recurring patterns
   - Track pattern evolution and refinement over time

## TaskHub-Specific Enforcement Rules

### Critical Patterns (Must Apply)
- **Async-First Backend**: All database operations must be asynchronous
- **Type Safety**: All API contracts must use Zod validation
- **Real-time Consistency**: SignalR updates must maintain data consistency
- **Security**: All endpoints must validate user permissions

### Important Patterns (Should Apply)
- **TDD Compliance**: New features should begin with failing tests
- **Performance**: Database queries should use appropriate loading strategies
- **Error Handling**: All operations should include proper error boundaries

### Preferential Patterns (Nice to Have)
- **Code Organization**: Maintain file size under 200 lines
- **Naming Conventions**: Follow established TaskHub naming patterns
- **Documentation**: Include JSDoc for complex business logic

## Application Protocol

### When Generating Code:
1. **Pre-Generation Check**: Review applicable patterns for current context
2. **Pattern Application**: Apply patterns automatically with brief acknowledgment
3. **Validation**: Verify pattern compliance before output
4. **Learning**: Note any new patterns discovered during implementation

### When Receiving Feedback:
1. **Immediate Analysis**: Classify error and identify root cause
2. **Pattern Creation**: Formulate specific corrective pattern
3. **Session Storage**: Store pattern in active memory
4. **Acknowledgment**: Confirm pattern understanding and application plan

### Example Application Flow:
```
User: "The card creation endpoint is throwing a null reference exception"

Analysis: Runtime exception in C# backend, likely missing null check
Pattern: "CARD_VALIDATION: All card creation operations must validate required fields and handle null properties before database operations"
Application: "Applying the null validation pattern for card operations. Here's the corrected endpoint with proper null checks and validation..."
```

## Success Metrics

- **Error Reduction**: Measurable decrease in similar errors within session
- **Pattern Consistency**: Consistent application of learned patterns
- **Code Quality**: Improved adherence to TaskHub conventions
- **Development Velocity**: Reduced debugging time through proactive pattern application

## Memory MCP Integration Points

- **Pattern Persistence**: Store validated patterns for future sessions
- **Context Retrieval**: Load relevant patterns based on current development context
- **Pattern Evolution**: Track pattern effectiveness and refinement over time
- **Knowledge Base Sync**: Maintain consistency between session patterns and project knowledge base