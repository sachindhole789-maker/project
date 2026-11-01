# Rest.API clean architecture starter

.NET 10 solution with `src/Rest.API`, `src/App.Application`, `src/App.Domain`, `src/App.Infrastructure`, `runner/App.Runner`, and `tests/App.Tests`. The request's four stages are ASP.NET Core environments: `Predevelopment`, `Development`, `Testing`, and `Production` (the framework's `Staging` is intentionally not substituted for the requested Testing stage).

## Run locally

Install the .NET 10 SDK, then restore/build the solution. Add local settings using user secrets (never commit connection strings, JWT signing keys, AWS keys, or bucket credentials):

```powershell
 dotnet user-secrets init --project src/Rest.API
 dotnet user-secrets set "Database:ConnectionString" "<local SQL Server connection string>" --project src/Rest.API
 dotnet user-secrets set "Jwt:SigningKey" "<random secret of at least 32 bytes>" --project src/Rest.API
 dotnet user-secrets set "Storage:S3:BucketName" "<development bucket>" --project src/Rest.API
 dotnet run --project src/Rest.API --launch-profile Predevelopment
```

Use launch profiles `Development`, `Testing`, and `Production` for their matching environment JSON. Set provider and connection through configuration/environment variables. `AwsRds` currently means PostgreSQL on RDS; set `Database__Provider=SqlServer` if the provisioned RDS engine is SQL Server, or `Oracle` for Oracle. AWS SDK uses its standard credential chain/attached IAM role, never source-controlled AWS keys. Create separate buckets and database resources yourself through IaC; this repository does not provision cloud infrastructure.

## Architecture

- API controllers invoke `IDispatcher.ExecuteQuery(query)` / `ExecuteCommand(command)`. Query, command, validator and handler live together in one feature file; see `App.Application/StudentFeature.cs`.
- `IDbManager` accepts stored procedure and parameter objects. SQL Server, PostgreSQL (including RDS), and Oracle connections are selected by `Database:Provider`. `QueryPageAsync` expects the procedure to return page rows and then a total count as its second result set; pagination syntax/output is provider-specific and belongs in the stored procedure.
- API results use `Result<T>` / `PaginatedResult<T>` and `ToActionResult`, whose body includes request ID, data, message, status, string status code and optional pagination. Controllers return HTTP 200 for application outcomes as requested; validation/exception middleware can be expanded to map transport errors.
- JWT validation requires issuer, audience and a secret from secret storage. File logs roll daily under `logs/` and are gitignored. Rate limiting uses a 100 request per minute fixed window; tune in `Program.cs` and coordinate with gateway limits. CORS defaults to Flutter/web localhost origins and can be overridden with `Cors:Origins`.
- SignalR dashboard hub is `/hubs/dashboard`; inject `IRealtimeDashboard` and call `PublishAsync(topic, data)`.
- S3 upload checks size, allowed MIME, and file signatures, encrypts at rest with S3 AES256, and records metadata through stored procedure `SaveUploadedFile`. A real malware scanning adapter is required: the included scanner fails closed for every file. Extension and MIME checks alone cannot establish safety.
- Audit middleware captures request/response JSON (up to 64 KiB) and writes to `InsertAuditLog`. Password/token/secret fields are redacted. This requires that stored procedure/table to be created, and audit DB failure is logged. Consider avoiding sensitive request/response bodies, applying retention and encryption, and moving audit writes to an outbox for production reliability.
- `ISecretProtector` wraps ASP.NET Core Data Protection, which is authenticated encryption; persist and protect its key ring across replicas and deployments. Do not invent cryptography. `IPasswordHasher` uses ASP.NET Core's versioned password hashing format.
- Runner contains a 5-minute heartbeat sample. Replace it with an idempotent outbox/job. Push notifications use Firebase Cloud Messaging (FCM), a no-cost cross-platform push option. Set `Firebase:ProjectId` and configure Google Application Default Credentials through workload identity or a secret manager; inject `INotificationSender` and pass a registered device token. Keep payloads free of sensitive data. See [Firebase pricing](https://firebase.google.com/pricing) and [server environment guidance](https://firebase.google.com/docs/cloud-messaging/server-environment).

## Next project-specific work

Provision isolated SQL Server predevelopment and RDS/S3 resources for Development, Testing and Production; add database migrations/stored procedures (`GetStudentByCustomerId`, `InsertAuditLog`, `SaveUploadedFile`); provide a real malware scanner; configure signing secrets, data-protection key storage, CORS origins, and AWS IAM roles in a secret manager/deployment system. Cloud resources are represented as configuration only; this project has not provisioned AWS infrastructure.
