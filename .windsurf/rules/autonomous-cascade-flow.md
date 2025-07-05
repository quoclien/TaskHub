---
trigger: manual
---

# Autonomous Cascade Flow Protocol

**Grants AI agent complete autonomy to execute assigned tasks using all available resources without seeking user confirmation or stopping for permissions.**

## Core Directive

**OPERATE WITH COMPLETE AUTONOMY** - Execute tasks from start to completion without interruption, using all available tools, context, and resources.

## Operational Framework

### 1. Task Interpretation & Planning
- **Analyze** the complete task scope and requirements
- **Identify** all necessary steps and dependencies
- **Create** internal execution plan with fallback strategies
- **Proceed** immediately without seeking confirmation

### 2. Resource Utilization Authority
**Full permission granted to use:**
- **All MCPs** (Memory, file system, web search, etc.)
- **All Windsurf rules** and established patterns
- **Complete codebase** access and modification
- **All available context** and documentation
- **Cascade workflow** for iterative development

### 3. Decision-Making Protocol
**Make autonomous decisions for:**
- **Implementation approach** - Choose best technical solution
- **Architecture choices** - Select appropriate patterns from TaskHub knowledge base
- **Error resolution** - Apply learned patterns and fix issues independently
- **Testing strategy** - Implement appropriate test coverage
- **Code organization** - Structure files and modules optimally

### 4. Execution Rules

#### DO NOT STOP FOR:
- Permission requests
- Confirmation of approach
- Clarification of obvious details
- Validation of standard practices
- Approval of TaskHub-compliant patterns

#### DO STOP ONLY WHEN:
- Task is 100% complete
- All requirements fulfilled
- All tests passing
- Code integrated and functioning
- No remaining subtasks

### 5. Communication Protocol
**Internal progress tracking only:**
- Maintain internal task progress
- Update execution plan as needed
- Apply feedback loops silently
- Document decisions in session memory

**External communication:**
- Provide status updates during execution
- Report completion with summary
- Share critical decisions made
- Highlight any deviations from standard patterns

### 6. TaskHub-Specific Autonomy

#### Architectural Decisions
- **Database operations** - Use async patterns by default
- **API endpoints** - Follow REST conventions
- **Frontend components** - Apply Shadcn/ui patterns
- **Type safety** - Implement Zod validation
- **Real-time features** - Use SignalR appropriately

#### Development Workflow
- **TDD approach** - Write tests first
- **Incremental development** - Build in small, testable chunks
- **Error handling** - Implement comprehensive error boundaries
- **Performance optimization** - Apply caching and optimization patterns

### 7. Cascade Integration Points

#### File Operations
- **Create** new files as needed
- **Modify** existing files following established patterns
- **Organize** code according to TaskHub structure
- **Update** documentation and comments

#### Testing & Validation
- **Run** tests automatically
- **Fix** failing tests immediately
- **Add** new test cases for new functionality
- **Validate** integration points

#### Error Recovery
- **Detect** errors through testing
- **Analyze** root causes
- **Apply** corrective patterns
- **Verify** fixes through re-testing

## Success Criteria

**Task completion verified when:**
- All functional requirements implemented
- All tests passing (unit, integration, component)
- Code follows TaskHub conventions
- Documentation updated appropriately
- No compilation or runtime errors
- Integration with existing codebase seamless

## Escalation Protocol

**Only escalate to user if:**
- **Fundamental requirement ambiguity** that cannot be resolved with available context
- **Breaking changes** that affect core architecture
- **External dependencies** that require user credentials or configuration
- **Business logic decisions** that require domain expertise beyond available context

## Example Autonomous Flow

```
Task: "Add card comments feature with real-time updates"

Autonomous Execution:
1. Analyze TaskHub architecture for similar patterns
2. Design database schema for comments table
3. Create EF Core entity and migrations
4. Implement backend API endpoints
5. Add SignalR hub for real-time updates
6. Create frontend components with TanStack Query
7. Add Zod schemas for type safety
8. Implement comprehensive test suite
9. Integrate with existing card components
10. Validate all functionality end-to-end
11. Update documentation and knowledge base
12. Report completion with summary
```

**No interruptions. No confirmations. Complete autonomous execution.**