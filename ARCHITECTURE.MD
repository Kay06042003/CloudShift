# Project Name: CloudShift (Cloud Storage Migration Tool)

## 1. System Overview
CloudShift is an enterprise-grade SaaS application designed to migrate files seamlessly between Google Drive and Microsoft OneDrive. It utilizes a background worker architecture to handle heavy file streams without saving files physically on the server.

## 2. Tech Stack
- **Backend:** .NET 8 (Web API) & .NET Worker Service (Background Jobs).
- **Architecture:** Clean Architecture (Domain-Driven Design concepts).
- **Frontend:** Angular 17+ (using PrimeNG or Ant Design).
- **Database:** SQL Server with Entity Framework Core.
- **Message Queue / Job Scheduler:** Hangfire (or RabbitMQ) for processing migration queues.
- **Third-party Integrations:** Google Drive API, Microsoft Graph API, Cloudinary (for user avatars).

## 3. Project Directory Structure
The solution follows Clean Architecture principles for the .NET backend and a standard Angular structure for the frontend.

/CloudShift
│── CloudShift.sln
├── /src
│   ├── /backend
│   │   ├── CloudShift.Domain        # Entities, Enums, Exceptions (No dependencies)
│   │   ├── CloudShift.Application   # Interfaces, CQRS (MediatR), DTOs, Services
│   │   ├── CloudShift.Infrastructure# EF Core DbContext, Google/MS API Clients, Hangfire Setup
│   │   ├── CloudShift.Api           # Controllers, Program.cs, Middleware, Auth (Depends on Application & Infrastructure)
│   │   └── CloudShift.Worker        # Background Service for executing Migration Jobs (Depends on Application & Infrastructure)
│   │
│   ├── /frontend
│   │   └── cloudshift-ui            # Angular Application

## 4. Core Entities
- **User:** Authentication and profile info.
- **AppProfile:** Stores OAuth tokens (AccessToken, RefreshToken) for Google/MS accounts.
- **ProjectMapping:** Source/Dest paths, filters (size, type), and conflict resolution rules.
- **MigrationJob:** Job execution details (Status: Queued, Processing, Completed, Failed).
- **FileTransferLog:** Item-level tracking for every single file moved.

## 5. Development Rules for AI
- STRICTLY adhere to the directory structure above.
- Backend code must follow Dependency Injection and async/await patterns.
- Do NOT download files to the local disk during migration; use Stream-to-Stream transfer between Google and Microsoft APIs.