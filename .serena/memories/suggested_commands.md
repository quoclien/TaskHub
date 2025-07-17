# TaskHub Development Commands

## Build & Development
- `pnpm dev:web` - Start frontend development server
- `pnpm dev:api` - Start backend with hot reload
- `dotnet build apps/api/TaskHub.Api.csproj` - Build API
- `dotnet clean apps/api/TaskHub.Api.csproj` - Clean build artifacts

## Database & Migrations
- `dotnet ef database update` - Apply migrations
- `dotnet ef migrations add <name>` - Create new migration

## Testing
- `dotnet test` - Run backend tests
- `pnpm test:run` - Run frontend tests

## LSP & IDE Issues
- Restart C# language server if intellisense breaks
- Clean and rebuild on reference/indexing issues