# User Management System

## Overview

This project is a simple User Management System built with .NET 8. It consists of a backend Web API, a Blazor Server frontend, and supporting libraries. The system allows for creating, reading, updating, and deleting (CRUD) user operations, along with password validation and secure API key authentication between the frontend and backend.

## Project Structure & Assemblies

The solution contains the following projects:

### 1. UserManagementService
The backend ASP.NET Web API project.
- **Responsibilities**: Handles data persistence, business logic, and exposes RESTful endpoints.
- **Key Features**:
  - SQL Server database integration via Entity Framework Core.
  - API Key authentication (`ApiKeyMiddleware`) for securing endpoints.
  - Serilog integration for structured logging.
  - Swagger/OpenAPI documentation. [NOTE: Swagger requests are exempted from the API key check in dev mode]

### 2. UserManagementService.UI
The frontend application built with Blazor Server.
- **Responsibilities**: Provides a user-friendly web interface for testing the server functionality
- **Key Features**:
  - Interactive pages for listing, adding, and editing users.
  - Communicates with the backend API via `UserServiceClient`.

### 3. UserManagementService.Common
A shared class library (DTOs).

### 4. UserManagementService.Tests
The unit testing project.

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
    Navigate to the `ProjectSetupTool` directory and run the application. Follow the on-screen prompts to select your preferred SQL Server instance. The app will then automatically update the connection string in the main project and apply EF database migrations.

3.  **Run the Application**
    Once setup is complete, you need to run both the backend API and the frontend UI.
    
    - Configure multiple startup projects to run both `UserManagementService` and `UserManagementService.UI`.
      (Technically the setup is already in UserManagementService.slnLaunch.user and should be selectable in the configuration combobox)
    - Run two IDEs or something, idc
    

4.  **Access the Application**
    - The **Frontend** will typically be available at `https://localhost:7032` (check console output for exact port).
    - The **Backend API** Swagger UI is available at `https://localhost:7111/swagger` (in Development mode).


