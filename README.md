# User Management System

## Overview

This project is a comprehensive User Management System built with .NET 8. It consists of a backend Web API, a Blazor Server frontend, and supporting libraries. The system allows for creating, reading, updating, and deleting (CRUD) user operations, along with password validation and secure API key authentication between the frontend and backend.

## Project Structure & Assemblies

The solution contains the following projects:

### 1. UserManagementService
The backend ASP.NET Core Web API project.
- **Responsibilities**: Handles data persistence, business logic, and exposes RESTful endpoints.
- **Key Features**:
  - SQL Server database integration via Entity Framework Core.
  - API Key authentication (`ApiKeyMiddleware`) for securing endpoints.
  - Serilog integration for structured logging.
  - Swagger/OpenAPI documentation.

### 2. UserManagementService.UI
The frontend application built with Blazor Server.
- **Responsibilities**: Provides a user-friendly web interface for managing users.
- **Key Features**:
  - Interactive pages for listing, adding, and editing users.
  - Communicates with the backend API via `UserServiceClient`.
  - Real-time validation and responsive design.

### 3. UserManagementService.Common
A shared class library.
- **Responsibilities**: Contains common data structures used across the solution.
- **Key Features**:
  - Data Transfer Objects (DTOs) such as `UserDto`, `CreateUserDto`, and `UpdateUserDto`.
  - Shared validation attributes.

### 4. UserManagementService.Tests
The unit testing project.
- **Responsibilities**: Ensures code quality and reliability.
- **Key Features**:
  - Unit tests for controllers and services (e.g., `PasswordService`).
  - Verifies business logic and error handling.

### 5. ProjectSetupTool
A helper console application to streamline the development environment setup.
- **Responsibilities**: Automates database configuration and initialization.
- **Key Features**:
  - Detects local SQL Server instances.
  - Updates the `appsettings.json` connection string in the backend service.
  - Applies Entity Framework migrations to create/update the database schema.

## Setup Instructions

The project includes a **ProjectSetupTool** to automate the configuration process.

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, Express, or Developer edition)

### Step-by-Step Setup

1.  **Build the Solution**
    Open the solution `UserManagementService.sln` in Visual Studio or your preferred IDE and build the entire solution to ensure all dependencies are restored and compiled.

2.  **Run the Setup Tool**
    Navigate to the `ProjectSetupTool` directory and run the application. You can do this via the terminal:
    ```bash
    cd ProjectSetupTool
    dotnet run
    ```
    Follow the on-screen prompts to:
    - Select your SQL Server instance.
    - Automatically update the connection string.
    - Apply database migrations.

3.  **Run the Application**
    Once setup is complete, you need to run both the backend API and the frontend UI.
    
    **Option A: Visual Studio / IDE**
    - Configure multiple startup projects to run both `UserManagementService` and `UserManagementService.UI`.
    
    **Option B: Terminal**
    - Terminal 1 (Backend):
      ```bash
      cd UserManagementService
      dotnet run
      ```
    - Terminal 2 (Frontend):
      ```bash
      cd UserManagementService.UI
      dotnet run
      ```

4.  **Access the Application**
    - The **Frontend** will typically be available at `https://localhost:7032` (check console output for exact port).
    - The **Backend API** Swagger UI is available at `https://localhost:7111/swagger` (in Development mode).

