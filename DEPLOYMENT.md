# STL Compliance Render Deployment

This repository deploys to Render from git autodeploy. Local changes do not affect production until they are committed and pushed to the Render-linked production branch.

## Production Branch

- Current production branch: `main`
- Remote: `origin`
- Render autodeploy trigger: commit push
- Render services currently link to `https://github.com/tubearrteam/STLComplianceV2`
- API and worker services use Docker with repository root as `dockerContext`.
- Static sites use per-app `rootDir` values.

Do not manually trigger Render deploys while fixing production failures. Do not use `render deploys create` or API deploy-trigger commands for the normal production loop. Push a commit to `main` and let Render autodeploy start from that push.

## Services

Web APIs:

- `nexarr-api`
- `staffarr-api`
- `trainarr-api`
- `maintainarr-api`
- `routarr-api`
- `supplyarr-api`
- `ledgarr-api`
- `compliancecore-api`
- `loadarr-api`
- `assurarr-api`
- `recordarr-api`
- `customarr-api`
- `ordarr-api`
- `reportarr-api`

Background workers:

- `shared-worker`
- `nexarr-worker`
- `staffarr-worker`
- `trainarr-worker`
- `maintainarr-worker`
- `routarr-worker`
- `supplyarr-worker`
- `ledgarr-worker`
- `compliancecore-worker`

Static sites:

- `fieldcompanion-frontend`
- `stlcompliancesite`
- `stlcompliancekb`

## Required Environment Variable Names

Runtime source of truth is `render.yaml`. Secret values must stay in Render and must not be committed.

Shared/auth/AI names:

- `ASPNETCORE_ENVIRONMENT`
- `LOG_LEVEL`
- `OTEL_ENABLED`
- `OTEL_EXPORTER_OTLP_ENDPOINT`
- `Cors__AllowedOriginPatterns`
- `AUTH_SIGNING_KEY`
- `SERVICE_TOKEN_SIGNING_KEY`
- `Auth__Issuer`
- `Auth__Audience`
- `SERVICE_TOKEN_ISSUER`
- `SERVICE_TOKEN_AUDIENCE`
- `STL_INTEGRATION_BOOTSTRAP_SECRET`
- `OPENAI_API_KEY`
- `OPENAI_ASSISTANT_MODEL`
- `OPENAI_SMART_IMPORT_MODEL`
- `OPENAI_ASSISTANT_VECTOR_STORE_IDS`
- `OPENAI_ASSISTANT_FILE_SEARCH_MAX_RESULTS`
- `OPENAI_REQUESTS_PER_MINUTE`
- `OPENAI_TOKENS_PER_MINUTE`
- `OPENAI_TIMEOUT_SECONDS`
- `OPENAI_RETRY_ATTEMPTS`

Common service names:

- `DATABASE_URL`
- `REDIS_URL`
- `STL_SERVICE_NAME`
- `STL_INTEGRATION_TOKEN_AUTO_PROVISION`
- `Handoff__ServiceToken`
- `NexArr__ServiceToken`
- `StaffArr__ServiceToken`
- `TrainArr__ServiceToken`
- `MaintainArr__ServiceToken`
- `RoutArr__ServiceToken`
- `SupplyArr__ServiceToken`
- `LedgArr__ServiceToken`
- `ComplianceCore__ServiceToken`
- `LoadArr__ServiceToken`
- `AssurArr__ServiceToken`
- `RecordArr__ServiceToken`
- `CustomArr__ServiceToken`
- `OrdArr__ServiceToken`
- `ReportArr__ServiceToken`

NexArr-specific names:

- `TENANT_INTEGRATION_ENCRYPTION_KEY`
- `RecordArr__ServiceToken`
- `SmartImport__DestinationServiceToken`
- `Seed__FirstAdminEmail`
- `Seed__FirstAdminPassword`
- `Seed__FirstAdminDisplayName`
- `FieldCompanionWebPush__Subject`
- `FieldCompanionWebPush__PublicKey`
- `FieldCompanionWebPush__PrivateKey`

Frontend/public URL names are maintained in `render.yaml` and `docs/deployment/ENVIRONMENT_VARIABLES.md`.

## Read-Only Troubleshooting Commands

Identify local git state:

```powershell
git branch --show-current
git remote -v
git log -1 --oneline --decorate
git status --short --branch
```

Inspect Render without triggering deploys:

```powershell
render whoami -o json
render workspace current -o json
render services -o json
render deploys list <service-id> -o json
render logs --resources <service-id> --start <utc-start> --end <utc-end> --limit 1000 -o text
render logs --resources <service-id> --level error --limit 100 -o text
```

Useful local validation:

```powershell
dotnet ef migrations has-pending-model-changes --project apps/nexarr-api/NexArr.Api/NexArr.Api.csproj --startup-project apps/nexarr-api/NexArr.Api/NexArr.Api.csproj --context NexArrDbContext
dotnet build apps/nexarr-api/NexArr.Api/NexArr.Api.csproj -c Release
```

When a deploy fails, read the failing Render logs first and classify the blocker before editing code: dependency/install, monorepo path, missing script, build/typecheck, env var name, migration/predeploy, start command, PORT binding, health check, Dockerfile, Blueprint validation, or dashboard-only configuration.
