# TaskHub Board Components

This directory contains the core UI components for TaskHub's kanban board functionality. All components follow the frontend best practices with TypeScript, proper component structure, and modern UI patterns.

## Components Overview

### Core Components

#### `BoardCard.tsx`
- **Purpose**: Displays board information in a grid layout
- **Features**: Hover effects, member count badge, relative time display
- **Props**: `id`, `title`, `memberCount`, `lastActivity`, `className`
- **Usage**: Board listing pages, board selection

#### `BoardView.tsx`
- **Purpose**: Horizontal scrolling container for kanban lists
- **Features**: Smooth scrolling, snap behavior, loading states
- **Props**: `children`, `isLoading`, `className`
- **Usage**: Main board view layout

#### `CardComponent.tsx`
- **Purpose**: Individual task cards within lists
- **Features**: Labels, due dates, member avatars, hover effects
- **Props**: `id`, `title`, `labels`, `dueDate`, `members`, `boardId`, `className`
- **Usage**: Task representation in kanban lists

#### `ListComponent.tsx`
- **Purpose**: Kanban list containers with cards
- **Features**: Header actions, card sorting, add card functionality
- **Props**: `id`, `title`, `position`, `cards`, `boardId`, action callbacks
- **Usage**: Column representation in kanban boards

### Modal Components

#### `CreateBoardModal.tsx`
- **Purpose**: Modal for creating new boards
- **Features**: Form validation with Zod, React Hook Form integration
- **Props**: `children`, `onSuccess`
- **Usage**: Board creation workflows

#### `CreateCardModal.tsx`
- **Purpose**: Modal for creating new cards
- **Features**: Title/description fields, form validation
- **Props**: `listId`, `boardId`, `cardCount`, `children`, `onSuccess`
- **Usage**: Card creation workflows

### Utility Components

#### `LoadingSkeleton.tsx`
- **Purpose**: Loading states for various components
- **Features**: Multiple skeleton types (card, list, text)
- **Components**: `SkeletonCard`, `SkeletonList`, `SkeletonText`
- **Usage**: Loading state placeholders

## Design Patterns

### Component Structure
All components follow this consistent structure:
1. **Imports** (External, Internal)
2. **Types & Interfaces**
3. **Component Documentation**
4. **Component Function**
5. **State & Hooks**
6. **Event Handlers**
7. **Render Helpers**
8. **Main Component Return**

### Styling Approach
- **Tailwind CSS**: Utility-first styling
- **Shadcn/ui**: Pre-built component primitives
- **Design System**: Consistent spacing, colors, typography
- **Responsive**: Mobile-first responsive design
- **Dark Mode**: Built-in theme support

### State Management
- **TanStack Query**: Server state management
- **Zustand**: Client state (auth store)
- **React Hook Form**: Form state management
- **Local State**: Component-specific state with `useState`

## Development

### Testing
All components include comprehensive test suites with:
- **Unit Tests**: Component behavior testing
- **Integration Tests**: Component interaction testing
- **Accessibility Tests**: ARIA compliance testing
- **Visual Regression**: Component appearance testing

### Performance
- **Code Splitting**: Lazy loading for large components
- **Memoization**: `React.memo` for expensive renders
- **Optimistic Updates**: Immediate UI feedback
- **Skeleton Loading**: Progressive loading experience

## Usage Examples

### Basic Board Card
```tsx
<BoardCard
  id="board-123"
  title="Project Alpha"
  memberCount={5}
  lastActivity="2024-01-20T10:30:00Z"
/>
```

### Kanban Board Layout
```tsx
<BoardView>
  <ListComponent
    id="list-1"
    title="To Do"
    position={0}
    cards={cards}
    boardId="board-123"
    onAddCard={handleAddCard}
  />
</BoardView>
```

### Create Board Modal
```tsx
<CreateBoardModal onSuccess={handleBoardCreated}>
  <Button>Create New Board</Button>
</CreateBoardModal>
```

## Testing

Run tests for board components:
```bash
pnpm test src/components/board
```

View component showcase:
```bash
# Start dev server and visit /test-components
pnpm dev
```

## Future Enhancements

- **Drag & Drop**: React DnD integration for card/list reordering
- **Real-time Updates**: SignalR integration for live collaboration
- **Advanced Filtering**: Filter cards by labels, members, due dates
- **Board Templates**: Pre-configured board layouts
- **Accessibility**: Enhanced keyboard navigation and screen reader support 