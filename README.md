# IpWatcher

A small .NET Worker Service that periodically checks the machine’s public IP address and sends an email notification when it changes. Data and logs are stored in SQLite.

## What it does

- Periodically calls a public IP provider (Ipify).
- Compares the current public IP to the last stored IP.
- If it’s the first run (no previous IP), or the IP changed:
  - Sends an email.
  - Persists the IP in SQLite.
- Writes structured logs to the same SQLite database.

Key flow lives in [`CheckIpChangeUseCase`](IpWatcher.Application/UseCases/CheckIpChangeUseCase.cs).

---

## Prerequisites

- Windows
- .NET SDK (the repo targets the installed SDK on your machine)
- An SMTP mailbox (host/port/credentials)

---

## Quick start (run as a console app)

From the repository root:

````powershell
cd c:\Github\IpWatcher
dotnet build
dotnet run --project .\IpWatcher.Worker\IpWatcher.Worker.csproj
````

## Architecture

This solution follows a “Clean Architecture”-style separation:

- **IpWatcher.Worker (Host / Composition Root)**
  - Responsible for: process hosting, DI wiring, scheduling (Quartz), and logging configuration.
  - Contains the job entrypoint that triggers the use case on a schedule.
- **IpWatcher.Application (Use cases + abstractions)**
  - Contains business workflow (e.g., “check IP change, decide what to do”) and interfaces such as storage/notifier/public IP provider.
  - Does **not** depend on Infrastructure.
- **IpWatcher.Infrastructure (Adapters / implementations)**
  - Implements Application abstractions:
    - Public IP provider (Ipify)
    - Email notifier (MailKit)
    - Persistence (EF Core + SQLite)
    - Log persistence (SQLite `LogEvents`)
- **IpWatcher.Domain (Domain model)**
  - Value objects and domain concepts (e.g., IP address).

**Dependency direction (important):**
`Worker` → `Infrastructure` → `Application` → `Domain`  
`Application` depends only on `Domain` (and its own abstractions), not on `Infrastructure`.

### Request flow (high level)

1. Quartz trigger fires `CheckIpChangeJob`
2. Job calls `CheckIpChangeUseCase` (Application)
3. Use case:
   - asks `IPublicIpProvider` for current public IP
   - asks `IIpStorage` for last stored IP
   - if changed (or first run): stores new IP and calls `IEmailNotifier`
4. Logs are written using Serilog and stored in SQLite (`LogEvents` table).

---

## Tech stack / versions

### .NET version

This repo targets the .NET version specified in the project file(s). To check:

- Open `IpWatcher.Worker/IpWatcher.Worker.csproj`
- Look for: `<TargetFramework>netX.Y</TargetFramework>`

Document it here once confirmed, for example:

- **.NET:** `net10.0` (example; replace with your actual target)

### Main libraries / components

- Hosting: .NET Worker Service (`Host.CreateApplicationBuilder`)
- Scheduling: Quartz.NET
- Persistence: EF Core + SQLite (+ migrations on startup)
- Email: MailKit (SMTP)
- Logging: Serilog → SQLite table `LogEvents`

---

## Testing

### Unit tests (Application)

Unit tests focus on the **use case behavior** (business rules) without external I/O:

- Style: Arrange–Act–Assert (AAA)
- Dependencies are replaced with fakes/mocks:
  - fake `IPublicIpProvider` returning deterministic IPs
  - fake `IIpStorage` to simulate previous IP present/absent
  - fake/mock `IEmailNotifier` to assert “email sent” vs “not sent”
- Goal: verify decisions and side effects (store IP / send email) for cases:
  - first run (no previous IP)
  - IP unchanged
  - IP changed

To see the exact framework and helpers used, inspect:
- `*.Tests/*.csproj` → `PackageReference` (xUnit/NUnit/MSTest, FluentAssertions, Moq/NSubstitute, etc.)

Run:
```powershell
dotnet test
```

### Integration tests (Worker / end-to-end)

Integration tests validate **wiring + real components** together:

Typical techniques used in this kind of Worker solution:

- Bootstrapping the Worker DI container in a test host
- Using a real SQLite database:
  - either a temporary file DB per test run, or `:memory:` (if configured)
- Exercising the end-to-end flow:
  - run the use case (or job) with real EF Core storage
  - ensure IP history is persisted
  - ensure an email notifier is invoked (usually replaced with a test implementation to avoid sending real email)

Check the integration test project to confirm the exact approach:
- `IpWatcher.Worker.Tests` project and its test fixtures

Run:
```powershell
dotnet test
```