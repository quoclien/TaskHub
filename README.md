# TaskHub: A Modern Trello-Clone

## Overview

TaskHub is an open-source Trello-like application designed for efficient task management and real-time collaboration. Built with a modern technology stack, it provides a seamless experience for organizing projects, tracking tasks, and collaborating with your team.

This project aims to demonstrate a robust full-stack application with a strong focus on clean architecture, real-time features, and a smooth user experience.

## Features (v1.0)

* **Board Management:** Create, view, and manage project boards.
* **List & Card Management:** Organize tasks within lists and detail them with cards.
* **Real-time Updates:** See changes instantly as collaborators interact with boards.
* **User Authentication:** Secure user login and registration with Google OAuth.
* **Drag-and-Drop Interface:** Intuitively move cards and lists for easy organization.

## Technology Stack

### Frontend
* Next.js 15.3 (App Router, React 19)
* TypeScript 5.6
* Shadcn/ui & Tailwind CSS
* TanStack Query v5
* React Hook Form with Zod
* @dnd-kit/core
* NextAuth.js v5

### Backend
* ASP.NET Core 9 (Minimal APIs)
* Entity Framework Core 9
* PostgreSQL (with JSONB)
* SignalR
* Serilog

### DevOps & Tools
* Docker
* GitHub Actions
* ESLint 9 & Prettier
* xUnit, Jest, Playwright

## Getting Started

To get a local copy up and running, follow these simple steps.

### Prerequisites

* Node.js (LTS recommended)
* .NET SDK 9
* Docker
* Git

### Installation

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/your-username/TaskHub.git](https://github.com/your-username/TaskHub.git)
    cd TaskHub
    ```

2.  **Set up environment variables:**
    Create a `.env.local` file in the `apps/web` directory for frontend environment variables (e.g., `NEXTAUTH_URL`, `NEXTAUTH_SECRET`, `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`).
    Create an `appsettings.Development.json` file in the `apps/api` directory for backend environment variables (e.g., database connection string).

3.  **Start Docker containers (for PostgreSQL):**
    ```bash
    docker-compose up -d
    ```

4.  **Run database migrations (for backend):**
    ```bash
    # Navigate to the API project directory (e.g., apps/api)
    dotnet ef database update
    ```

5.  **Install dependencies and run the applications:**

    ```bash
    # Install shared packages
    cd packages/shared
    npm install # or yarn install
    cd ../..

    # Install web frontend dependencies and run
    cd apps/web
    npm install # or yarn install
    npm run dev
    cd ../..

    # Run the API backend
    cd apps/api
    dotnet run
    cd ../..
    ```

    The frontend will typically run on `http://localhost:3000` and the backend API on `http://localhost:5205` (or as configured).

## Contributing

We welcome contributions! If you'd like to contribute, please fork the repository and create a pull request. We're open to new features, bug fixes, and improvements.

## License

This project is open-source and licensed under the [MIT License](LICENSE).

---
*TaskHub is an open-source project. This is the initial release (v1.0).*