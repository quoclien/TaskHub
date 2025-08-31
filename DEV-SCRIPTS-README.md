# TaskHub Development Scripts

This directory contains scripts to streamline your development workflow by automating the startup and shutdown of all required services.

## Scripts Overview

### `start-dev.ps1` - Main Development Startup Script ⭐

**Primary script for starting your development environment**

- ✅ Checks all prerequisites (Docker, .NET, PNPM, Node.js)
- 🐳 Starts PostgreSQL database in Docker
- 🔄 Applies Entity Framework migrations
- 📦 Installs/updates dependencies
- ⚡ Launches backend API server in separate terminal window
- 🌐 Launches frontend dev server in separate terminal window
- 📋 Shows summary with all service URLs

#### Usage Examples

```powershell
# Start all services (recommended)
.\start-dev.ps1

# Start only frontend
.\start-dev.ps1 -Frontend

# Start only backend
.\start-dev.ps1 -Backend

# Show help
.\start-dev.ps1 -Help
```

### `stop-dev.ps1` - Development Cleanup Script

**Stops all development services and containers**

- 🛑 Stops Docker containers
- 📊 Shows running development processes
- ℹ️ Provides guidance on stopping manual processes

#### Usage Examples

```powershell
# Stop all services
.\stop-dev.ps1

# Show help
.\stop-dev.ps1 -Help
```

## Prerequisites

Before running the scripts, ensure you have:

### Required Software
- **Docker Desktop** - For PostgreSQL database
- **.NET 8+ SDK** - For backend API
- **Node.js 18+** - For frontend
- **PNPM 8+** - Package manager
- **Windows Terminal** (recommended) - For better terminal experience

### Initial Setup
1. **Enable PowerShell script execution:**
   ```powershell
   Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

2. **Ensure Docker Desktop is running** before starting development

## Development Workflow

### 🚀 Starting Development

1. **Quick Start (Recommended):**
   ```powershell
   .\start-dev.ps1
   ```
   This will start everything you need for development.

2. **The script will:**
   - Check if all tools are installed
   - Start PostgreSQL database in Docker
   - Apply any pending database migrations
   - Install/update dependencies
   - Open 2 new terminal windows:
     - Backend API server (http://localhost:5205)
     - Frontend dev server (http://localhost:3000)

3. **Access your application:**
   - **Frontend:** http://localhost:3000
   - **Backend API:** http://localhost:5205  
   - **API Documentation:** http://localhost:5205/swagger
   - **Database:** localhost:5432 (postgres/postgres)

### 🛑 Stopping Development

1. **Stop all services:**
   ```powershell
   .\stop-dev.ps1
   ```

2. **Manual shutdown:**
   - Use `Ctrl+C` in each terminal window to stop servers
   - Run `docker-compose down` to stop containers

### ⚡ Partial Restarts

- **Frontend only:** `.\start-dev.ps1 -Frontend`
- **Backend only:** `.\start-dev.ps1 -Backend`

## Troubleshooting

### Common Issues

**Script execution policy error:**
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Docker not running:**
- Start Docker Desktop
- Wait for it to fully initialize
- Re-run the script

**Port conflicts:**
- Check if ports 3000, 5205, or 5432 are in use
- Stop other applications using these ports
- Or modify the configuration

**Migration failures:**
- Ensure the database container is running
- Check connection string in appsettings
- Manually run: `dotnet ef database update` in `/apps/api`

**Windows Terminal not found:**
- The script will fall back to regular PowerShell windows
- Install Windows Terminal for a better experience

### Manual Commands

If scripts fail, you can run commands manually:

```powershell
# Start database
docker-compose up -d db

# Start backend (in apps/api directory)
dotnet watch --project apps/api/TaskHub.Api.csproj

# Start frontend (in project root)
pnpm --filter @taskhub/web dev

# Apply migrations (in apps/api directory)
dotnet ef database update

# Stop all containers
docker-compose down
```

## Script Locations

### Core Development Scripts
- `start-dev.ps1` - Main comprehensive startup script
- `stop-dev.ps1` - Development environment cleanup script

### Testing Scripts
- `validate-all-tests.ps1` - Comprehensive test validation pipeline
- `test-e2e-setup.ps1` - End-to-end testing environment setup

### Utility Scripts
- `apps/api/src/_cleanup/Remove-OldDtos.ps1` - One-time cleanup utility
- `DEV-SCRIPTS-README.md` - This documentation

## Tips

- **First run:** Let the full startup complete before making changes
- **Development:** Use `Ctrl+C` in terminal windows to restart individual services
- **Debugging:** Backend runs with hot reload (`dotnet watch`)
- **Frontend:** Frontend has hot reload enabled by default
- **Database:** Data persists between restarts (Docker volume)

---

**Happy coding! 🎉**

For more information about the project, see the main [README.md](./README.md). 