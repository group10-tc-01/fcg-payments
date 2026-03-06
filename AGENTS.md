# AGENTS.md

## Purpose
- This repository is a .NET 8 payments service built with Clean Architecture.
- Main layers live under `src/`: `WebApi`, `Application`, `Domain`, and `Infrastructure.*`.
- Tests live under `tests/` with separate unit, integration, and shared test utility projects.
- Use this file as the default operating guide for coding agents working in this repo.

## Current Rule Files
- No `.cursor/rules/` directory was found.
- No `.cursorrules` file was found.
- No `.github/copilot-instructions.md` file was found.
- If any of those files are added later, treat them as higher-priority repo instructions and update this file.

## Solution Layout
- Solution file: `FCG.Payments.slnx`.
- API entrypoint: `src/FCG.Payments.WebApi/Program.cs`.
- Application use cases: `src/FCG.Payments.Application/UseCases/...`.
- Domain entities, value objects, and exceptions: `src/FCG.Payments.Domain/...`.
- Infrastructure adapters: `src/FCG.Payments.Infrastructure.*`.
- Unit tests: `tests/FCG.Payments.UnitTests`.
- Integration tests: `tests/FCG.Payments.IntegratedTests`.
- Shared test builders and mocks: `tests/FCG.Payments.CommomTestUtilities`.

## Environment And Runtime Notes
- Target framework is `net8.0` across all projects.
- `Nullable` and `ImplicitUsings` are enabled in project files.
- Local infrastructure is described in `docker-compose.yml`.
- Main external dependencies are SQL Server, Kafka, and Seq.
- Integration tests replace the production database with in-memory SQLite via `CustomWebApplicationFactory`.
- Integration tests also remove Kafka registrations and run the API with environment `Test`.

## Preferred Working Flow
1. Read the relevant layer before editing; preserve Clean Architecture boundaries.
2. Make the smallest safe change that matches existing conventions.
3. Run restore/build/tests for the smallest affected scope first.
4. At the end of every implementation task, run at least a build for the smallest affected scope to catch compilation errors before asking for review.
5. After each completed implementation task, create a git commit before moving to the next task, using a conventional commit type like `feat`, `fix`, or `refactor` with a short English explanation of why the change was made.
6. If you touch validation, exceptions, controllers, or persistence, run the related integration and unit tests.
7. Do not edit generated files unless the task is explicitly about migrations or generated output.

## Build Commands
- Restore solution: `dotnet restore FCG.Payments.slnx`
- Build solution: `dotnet build FCG.Payments.slnx --configuration Release`
- Build without restore: `dotnet build FCG.Payments.slnx --no-restore --configuration Release`
- Build only API project: `dotnet build src/FCG.Payments.WebApi/FCG.Payments.WebApi.csproj --configuration Release`
- Build only unit tests: `dotnet build tests/FCG.Payments.UnitTests/FCG.Payments.UnitTests.csproj --configuration Release`
- Build only integration tests: `dotnet build tests/FCG.Payments.IntegratedTests/FCG.Payments.IntegratedTests.csproj --configuration Release`

## Run Commands
- Run API locally: `dotnet run --project src/FCG.Payments.WebApi/FCG.Payments.WebApi.csproj`
- Run Docker stack: `docker compose up -d`
- Stop Docker stack: `docker compose down`
- Rebuild Docker stack: `docker compose up -d --build`

## Test Commands
- Run all tests: `dotnet test FCG.Payments.slnx --configuration Release`
- Run unit tests only: `dotnet test tests/FCG.Payments.UnitTests/FCG.Payments.UnitTests.csproj --configuration Release`
- Run integration tests only: `dotnet test tests/FCG.Payments.IntegratedTests/FCG.Payments.IntegratedTests.csproj --configuration Release`
- Run with no rebuild after a prior build: `dotnet test tests/FCG.Payments.UnitTests/FCG.Payments.UnitTests.csproj --no-build --configuration Release`
- Collect coverage like CI for unit tests:
  `dotnet test tests/FCG.Payments.UnitTests/FCG.Payments.UnitTests.csproj --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults/UnitTests --logger trx --verbosity normal -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover`
- Collect coverage like CI for integration tests:
  `dotnet test tests/FCG.Payments.IntegratedTests/FCG.Payments.IntegratedTests.csproj --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults/IntegrationTests --logger trx --verbosity normal -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover`

## Running A Single Test
- By fully qualified name:
  `dotnet test tests/FCG.Payments.UnitTests/FCG.Payments.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~FCG.Payments.UnitTests.Application.UseCases.Payments.ProcessPaymentUseCaseTest.Given_Valid_Request_With_Sufficient_Balance_When_ProcessPaymentIsCalled_Then_Should_Approve_Payment"`
- By class:
  `dotnet test tests/FCG.Payments.UnitTests/FCG.Payments.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~ProcessPaymentUseCaseTest"`
- By method substring:
  `dotnet test tests/FCG.Payments.IntegratedTests/FCG.Payments.IntegratedTests.csproj --configuration Release --filter "FullyQualifiedName~Given_ValidRequest_When_GetPaymentHistoryIsCalled_ShouldReturnOk"`
- xUnit traits are not used in the current tests; prefer `FullyQualifiedName` filters.

## Lint And Formatting
- There is no dedicated lint script, `dotnet format` config, or CI lint job in the repo today.
- The practical quality gate is: restore, build, unit tests, integration tests, and coverage threshold in CI.
- Use the repo `.editorconfig` as the formatting authority.
- If a formatting check is needed and `dotnet format` is available in the environment, use:
  `dotnet format FCG.Payments.slnx --verify-no-changes`
- If `dotnet format` is unavailable, make code match neighboring files and `.editorconfig` exactly.

## CI Expectations
- PR CI builds the solution in `Release`.
- PR CI runs unit tests and integration tests separately.
- CI collects OpenCover coverage artifacts.
- Coverage threshold is 80%; CI fails below that threshold.
- Sonar analysis also runs after a successful build.
- Branch names must match Git Flow style: `feature/*`, `release/*`, or `hotfix/*`.

## Code Style: Formatting
- Use 4 spaces for indentation.
- Use CRLF line endings if your editor can preserve them.
- Do not insert a final newline automatically; `.editorconfig` disables it for C# files.
- Keep braces on new lines; the repo uses Allman-style blocks.
- Use block-scoped namespaces, not file-scoped namespaces.
- Keep `using` directives outside the namespace.
- Do not force `System.*` usings to the top; `.editorconfig` disables that preference.

## Code Style: Imports And Dependencies
- Keep imports minimal and remove unused usings.
- Follow existing dependency direction: `WebApi -> Application -> Domain`, with infrastructure implementing abstractions.
- Do not reference infrastructure from domain.
- Prefer constructor injection for services, repositories, loggers, and unit of work abstractions.
- Register cross-cutting concerns through dependency injection extension methods.

## Code Style: Types And Nullability
- Keep nullability annotations accurate; all projects have nullable reference types enabled.
- Prefer explicit types over `var`; `.editorconfig` disables `var` preference in all common cases.
- Use `sealed` on concrete classes when the surrounding code already does so.
- Keep DTOs, requests, and responses small and purpose-specific.
- Use value objects like `Amount` and `Balance` instead of leaking raw validation rules everywhere.

## Code Style: Naming
- Use PascalCase for types, methods, and properties.
- Prefix interfaces with `I`.
- Match namespaces to folder paths.
- Request/response/use-case naming is consistent: `CreateWalletRequest`, `CreateWalletResponse`, `CreateWalletUseCase`.
- Test classes usually end with `Test`, not `Tests`.
- Test methods use descriptive `Given_When_Then` names; continue that style.

## Code Style: Domain And Application Patterns
- Keep business rules inside domain entities and value objects when possible.
- Use static factory methods for aggregate creation when the domain already exposes them, such as `Wallet.CreateWallet` and `Payment.CreatePayment`.
- Application use cases orchestrate repositories, logging, and unit of work; they should not absorb low-level infrastructure concerns.
- Validation belongs in FluentValidation validators and MediatR pipeline behaviors.
- Pagination rules belong in request validators or pagination abstractions, not controllers.

## Error Handling
- Prefer domain-specific exceptions such as `DomainException`, `NotFoundException`, `ConflictException`, and `UnauthorizedException`.
- Throw `ValidationException` through FluentValidation behavior instead of hand-rolling controller validation.
- Let `GlobalExceptionMiddleware` translate exceptions into HTTP responses.
- For unexpected exceptions, the middleware returns `ProblemDetails` with `traceId` and optional `correlationId`.
- Keep exception messages user-readable and specific, following the existing English phrasing.

## Logging And Observability
- Use `ILogger<T>` via DI.
- Existing application code logs important state transitions with structured logging placeholders.
- Prefer structured logging like `LogInformation("Payment approved for UserId: {UserId}", userId)` over string interpolation.
- Preserve correlation-aware flows when touching middleware or Kafka/event code.

## Testing Conventions
- Use xUnit for test execution.
- Use FluentAssertions for assertions.
- Use Moq and builder helpers from `tests/FCG.Payments.CommomTestUtilities` for setup.
- Keep Arrange/Act/Assert sections explicit; the test suite uses comments for those sections.
- For API tests, use `CustomWebApplicationFactory` and authenticated helper methods from the fixture.
- For new tests, mirror the folder structure of the production code when practical.

## Persistence And Migrations
- EF Core persistence code lives under `src/FCG.Payments.Infrastructure.SqlServer/Persistance`.
- Keep the existing `Persistance` spelling for consistency, even though it is non-standard English.
- Repository methods are async and typically return nullable entities or tuples for paginated results.
- When changing entity shape, update EF configuration and migrations together.
- Do not hand-edit migration designer files unless regeneration is impossible.

## Agent-Specific Advice
- Before broad refactors, search for parallel implementations in wallets, payments, controllers, validators, and tests.
- Preserve public API routes and response wrappers unless the task explicitly changes the contract.
- If you add new behaviors or policies, wire them through the existing DI extension pattern.
- If local `dotnet` is unavailable in your environment, still keep commands in your notes and explain they were not executed.

## Verification Checklist
- `dotnet restore FCG.Payments.slnx`
- `dotnet build FCG.Payments.slnx --configuration Release`
- `dotnet test tests/FCG.Payments.UnitTests/FCG.Payments.UnitTests.csproj --configuration Release`
- `dotnet test tests/FCG.Payments.IntegratedTests/FCG.Payments.IntegratedTests.csproj --configuration Release`
- Confirm branch naming still follows `feature/*`, `release/*`, or `hotfix/*` when opening a PR.
