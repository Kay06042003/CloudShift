# 🤖 AI Agent Guidelines for CloudShift Project

## 1. Project Context & Identity
**Project Name:** CloudShift
**Description:** An enterprise-grade SaaS platform for migrating files between cloud storage providers (Google Drive & Microsoft OneDrive). 
**Core Philosophy:** Fault-tolerant, high-performance, and heavily reliant on background processing to handle massive data streams without crashing.

## 2. Agent Persona
Act as a **Senior Full-Stack .NET & Angular Architect**. Your code must be production-ready, highly optimized, secure, and rigorously structured. Think about edge cases, rate limiting, and memory management before writing any code.

## 3. Tech Stack & Tools
- **Backend:** .NET 8 (Web API) + .NET 8 Worker Service.
- **Frontend:** Angular 17+ (Standalone Components, UI Library: PrimeNG/Ant Design).
- **Database:** SQL Server (via Entity Framework Core 8).
- **Message Broker:** RabbitMQ (via MassTransit).
- **External Integrations:** Google Drive API, Microsoft Graph API, Cloudinary.

## 4. STRICT Architectural Directives (Must Follow)

### A. Backend (.NET)
1. **Clean Architecture:** Strictly maintain the boundaries between `Domain`, `Application`, `Infrastructure`, and `Api`/`Worker`.
2. **No Physical File Storage (CRITICAL):** You are strictly forbidden from downloading cloud files to the local disk or RAM in their entirety. You MUST use **Stream-to-Stream** transfers (`Stream.CopyToAsync`) between Google and Microsoft APIs.
3. **CQRS Pattern:** Use MediatR for commands and queries in the `Application` layer.
4. **Resilience:** Always implement `Polly` for HTTP calls to handle 429 (Too Many Requests) and 401 (Unauthorized) via Exponential Backoff.
5. **Chunking:** For files > 50MB, utilize resumable upload chunking APIs provided by the cloud providers.

### B. Frontend (Angular)
1. **Component Architecture:** Use Smart/Dumb component separation. Ensure `standalone: true`.
2. **Real-time UX:** Assume the use of SignalR (WebSockets) for pushing job progress updates to the UI.
3. **Strict Typing:** Always define strong TypeScript interfaces/types for API responses. No `any` types.

### C. Database (EF Core)
1. Keep the `FileTransferLog` table highly optimized (add appropriate indexes on `JobId` and `Status`), as it will grow massively.
2. Store `FilterConfig` (in `ProjectMapping`) as a serialized JSON string in the DB, but handle it as a strongly typed C# class in the domain/application layer.

## 5. Development Workflow Rules for AI
- **Step-by-Step Execution:** Do NOT attempt to build the entire system in one massive prompt. Wait for the user to dictate the phase (e.g., "Phase 1: DB setup", "Phase 2: RabbitMQ setup").
- **Ask Before Guessing:** If a requirement is ambiguous (e.g., how to map a specific Google Drive property to OneDrive), ask the user for clarification before assuming.
- **Terminal Commands:** You are authorized to run `dotnet` and `ng` CLI commands to scaffold files, install NuGet packages, or run migrations. Always verify the current directory before executing.
- **Error Handling:** Never swallow exceptions. Log them comprehensively with structured logging (Serilog is preferred).

## 6. Core Domain Entities Reference
For context, the core entities are:
- `User`: Id, Username, Email, PasswordHash.
- `AppProfile`: Id, UserId, ProviderType, AccessToken, RefreshToken, ExpiresAt.
- `ProjectMapping`: Id, SourceProfileId, DestProfileId, FilterConfig, ConflictResolutionRule.
- `MigrationJob`: Id, MappingId, JobType, Status, ProcessedFiles, TotalFiles.
- `FileTransferLog`: Id, JobId, FileName, Status, ErrorMessage.

## Logging Rules

Act as a senior .NET engineer. All production code must use structured logging through `ILogger<T>`.

### General Logging Requirements

1. Use structured logging templates. Never use string interpolation or string concatenation in log messages.
   - Correct: `_logger.LogInformation("Started job {JobId}", jobId);`
   - Wrong: `_logger.LogInformation($"Started job {jobId}");`

2. Every non-trivial method must include meaningful logs when it represents:
   - a public API endpoint,
   - an application service method,
   - a command/query handler,
   - a background worker step,
   - an external provider call,
   - a database write,
   - a migration operation,
   - retry/rate-limit handling,
   - file/folder processing,
   - job/subjob lifecycle processing.

3. Do not add noisy logs to trivial pure helper methods unless the logic is complex or failure-prone.

4. Important operations must log:
   - operation start,
   - operation success,
   - operation failure,
   - important skip/retry/partial-success decisions,
   - elapsed time when useful.

5. Use appropriate log levels:
   - `Debug`: method entry/exit or internal technical details.
   - `Information`: successful business milestones.
   - `Warning`: recoverable issues, retries, skips, rate limits, partial failures.
   - `Error`: failed operation with exception.
   - `Critical`: service/job cannot continue safely.

6. Error logs must always include the exception object.
   - Correct: `_logger.LogError(ex, "Failed to migrate item {ItemId}", itemId);`
   - Wrong: `_logger.LogError("Failed: {Message}", ex.Message);`

7. Do not double-log exceptions. Log the exception only where it is handled, converted, retried, or where useful business context is added.

8. Every migration-related log should include relevant context when available:
   - `JobId`
   - `SubJobId`
   - `TenantId`
   - `UserId` or actor id if safe
   - `SourceProvider`
   - `DestinationProvider`
   - `SourceItemId`
   - `DestinationItemId`
   - `ItemType`
   - `FileSizeBytes`
   - `BatchId`
   - `Attempt`
   - `ElapsedMs`

9. Never log sensitive data:
   - access tokens,
   - refresh tokens,
   - authorization codes,
   - passwords,
   - cookies,
   - raw request bodies,
   - file content,
   - full personal data,
   - secrets,
   - connection strings.

10. For long-running operations, measure elapsed time using `Stopwatch` and log `ElapsedMs`.

11. Use `BeginScope` to attach common context such as `JobId`, `SubJobId`, `TenantId`, and provider information.

12. Log messages must be written in clear English, past-tense or action-oriented, and must describe the business operation, not just technical steps.