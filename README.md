# Reusable .NET Clean Architecture API Template

A reusable starter solution for .NET 10 REST APIs. Copy this repository or mark it as a GitHub template to start new services with the same baseline architecture.

## Solution layout

```text
.
├── App.slnx
├── src/
│   ├── Rest.API/                 # HTTP controllers, middleware, JWT, CORS, SignalR
│   ├── App.Application/          # Use cases, validators, handlers, dispatch
│   ├── App.Domain/               # Results, entities/contracts, repository ports
│   └── App.Infrastructure/       # Dapper, database providers, AWS S3, security
├── runner/
│   └── App.Runner/               # Background worker process
└── tests/
    └── App.Tests/                # Unit/integration test project
```

### Layer responsibilities

- **Rest.API**: HTTP concerns only. Controllers dispatch requests and wrap application results in the shared response envelope.
- **App.Application**: use cases, validation and orchestration. Each feature can keep its query/command, response, validator and handler together in one file. Example: `src/App.Application/StudentFeature.cs`.
- **App.Domain**: dependency-light contracts and result types, including `IDbManager`, `IObjectStorage`, `IRealtimeDashboard`, and security interfaces.
- **App.Infrastructure**: implementations for SQL Server, PostgreSQL/RDS, Oracle, S3, audit storage, password hashing and secret protection.
- **App.Runner**: independent hosted worker. Replace the sample heartbeat with a real idempotent task before production.
- **App.Tests**: add unit and integration tests here.

## Requirements and first run

Install the .NET 10 SDK. Clone or copy the template, then restore and build:

```powershell
git clone https://github.com/sachindhole789-maker/project.git MyService
Set-Location MyService
dotnet restore App.slnx
dotnet build App.slnx
```

Set the local secrets for the API. User Secrets are for local development; use your deployment platform's secret manager for hosted environments.

```powershell
dotnet user-secrets init --project src/Rest.API
dotnet user-secrets set "Database:ConnectionString" "<connection string>" --project src/Rest.API
dotnet user-secrets set "Jwt:SigningKey" "<random secret, at least 32 bytes>" --project src/Rest.API
dotnet user-secrets set "Storage:S3:BucketName" "<development bucket>" --project src/Rest.API
dotnet run --project src/Rest.API --launch-profile Predevelopment
```

The API intentionally refuses to start without a JWT signing key. Supply one through user secrets or an environment-specific secret manager; do not put a real signing key in `appsettings*.json`.

Run tests when you add them:

```powershell
dotnet test App.slnx
```

## Environments and configuration

Four ASP.NET Core environments are set up: `Predevelopment`, `Development`, `Testing`, and `Production`. Matching `appsettings.{Environment}.json` files and API launch profiles are provided. Choose an environment with a launch profile locally or `ASPNETCORE_ENVIRONMENT` when deploying.

Provider and secret configuration can be set with environment variables. Double underscore (`__`) maps to a configuration colon:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Database__Provider = "AwsRds"
$env:Database__ConnectionString = "<secret connection string>"
$env:Storage__S3__BucketName = "<development bucket>"
$env:AWS__Region = "<aws region>"
$env:Jwt__SigningKey = "<secret from secret manager>"
```

Environment intent in the sample settings:

| Environment | Database default | File storage |
|---|---|---|
| Predevelopment | SQL Server | None by default |
| Development | AWS RDS PostgreSQL | S3 |
| Testing | AWS RDS PostgreSQL | S3 |
| Production | AWS RDS PostgreSQL | S3 |

`AwsRds` selects PostgreSQL via Npgsql. Set `Database:Provider` to `SqlServer` or `Oracle` when using those engines. Dapper sends stored procedure names and parameter objects through `IDbManager`; each provider's stored procedure implementation and paging result sets must match its own database syntax. `QueryPageAsync` expects page rows followed by a total count as the second result set.

The JSON files contain empty resource names and connection strings. This repo does not create AWS databases, buckets, IAM roles, or secret-manager entries. Create separate resources for each environment and configure least-privilege IAM roles. The AWS SDK uses its normal credential chain, so prefer workload identity/instance roles over long-lived access keys.

## Adding a feature

1. Add a single feature file under `App.Application/Features/V1/<Feature>/`.
2. Define the request as `IQuery<TResponse>` or `ICommand<TResponse>` and add the response record, FluentValidation validator and handler in that file.
3. Inject the handler's required repository/service interfaces. Add those interfaces to Domain and implementations to Infrastructure.
4. Add an API controller action that calls `await dispatcher.ExecuteQuery(query)` or `ExecuteCommand(command)` and returns `this.ToActionResult(result)`.
5. Add tests for the handler and validation rules.

The sample `GET /api/v1/students/{customerId}` demonstrates the query path. The API response envelope has `RequestId`, `Data`, `Message`, `Status`, string `StatusCode`, and optional pagination. Application outcomes are returned with HTTP 200 as requested; request validation and unexpected exceptions use transport-level 400/500 responses.

## Included services and controls

- **Database**: Dapper stored-procedure manager with SQL Server, PostgreSQL (including RDS) and Oracle connection adapters. Select the provider per environment using configuration.
- **JWT**: bearer-token validation checks issuer, audience, signing key and lifetime. Add your authentication/login flow or identity provider to issue tokens; this template does not create user credentials or issue login tokens.
- **Rate limiting**: global fixed window, 100 requests per minute per authenticated subject or remote IP. Tune in `src/Rest.API/Program.cs` and align limits with any gateway.
- **CORS**: `FlutterDev` policy defaults to local Flutter/web development origins. Set `Cors:Origins` to your exact deployed frontend origins; avoid wildcard origins when credentials are enabled.
- **Logging**: Serilog writes console output and daily text files under `logs/`. Logs are gitignored. Set retention and log access/rotation policies for deployment.
- **Audit**: middleware records request metadata and JSON bodies/responses up to 64 KiB to the `InsertAuditLog` stored procedure. Common secret/password/token fields are redacted. Create the table/procedure, set retention, and consider minimizing body capture for private or regulated data. Audit persistence failures are logged.
- **Realtime dashboard**: SignalR hub at `/hubs/dashboard`. Inject `IRealtimeDashboard` and call `PublishAsync(topic, data)` from an application service.
- **Uploads**: `IDocumentUploadService` validates size, allowed content type and file signatures, uploads with S3 server-side encryption, then records metadata with `SaveUploadedFile`. The included malware scanner fails closed for all files. Implement and configure a real scanner before enabling uploads; signature/MIME/extension checks are not virus scanning.
- **Secrets and passwords**: `ISecretProtector` uses ASP.NET Core Data Protection (authenticated encryption), not custom cryptography. Persist and protect its key ring across replicas. `IPasswordHasher` uses ASP.NET Core's versioned password hasher. Store password hashes, never reversible encrypted passwords.
- **Push notifications**: optional Firebase Cloud Messaging adapter behind `INotificationSender`. Set `Firebase:ProjectId` and configure Google Application Default Credentials through workload identity or a secret manager. FCM is listed as no-cost; keep sensitive information out of notification payloads. See [Firebase pricing](https://firebase.google.com/pricing) and [FCM server guidance](https://firebase.google.com/docs/cloud-messaging/server-environment).

## Git and credential hygiene

- Git's `user.name` and `user.email` set commit authorship; they do **not** authenticate pushes.
- On Windows, Git Credential Manager can authenticate Git operations in the browser. Check the configured GitHub account with `git credential-manager github list`. Do not copy tokens or passwords into remotes, scripts, or source files.
- Authenticate with the GitHub account that has write access to the repository. Use a fine-grained token only if your organization requires one; grant the minimum repository access and contents-write permission. Let Git Credential Manager store it securely.
- Keep real connection strings, JWT keys, AWS credentials, Firebase service-account keys and personal data out of Git. Use User Secrets locally and a secret manager/workload identity in hosted environments. If a secret is accidentally committed, rotate it; deleting the file in a later commit does not remove it from history.
- `.gitignore` excludes build output, logs, local environment files and local settings. Review `git status` before committing and enable GitHub secret scanning/Dependabot where available.

## Reuse this repository

For a fresh service, either clone and remove the old Git history, or set this repository as a GitHub template under **Settings → General → Template repository** and use **Use this template**. Then update solution/project names, namespaces, service-specific connection/provider configuration, CORS origins, bucket names, signing secrets, database procedures, authentication issuer/audience, and CI/CD deployment settings. Keep this repository as the clean baseline so future APIs can start from it again.
