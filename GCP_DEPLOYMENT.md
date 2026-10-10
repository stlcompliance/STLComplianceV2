# Google Cloud Run deployment and recovery

The repository's default deployment remains Render. This is the separate, opt-in Google Cloud Run pipeline. Local edits do not change either deployed environment.

## Investigation — October 10, 2026

Read-only inspection of project `stl-compliance`, region `us-central1`, found:

- `routarr-api` and `nexarr-worker` have failed startup revisions. The worker host previously had no HTTP listener at all. Retained RoutArr logs contain the platform startup failure, but no application exception proving its original cause.
- `stlcompliancesite` and `stlcompliancekb` point to image tags absent from Artifact Registry.
- The suite frontend returns HTTP 200 **HTML** for `/api/v1/health`. The former Nginx image had no API proxy.
- Cloud SQL `stl-compliance-v1` is **SUSPENDED**, with activation policy `ALWAYS`. Project billing reports enabled; the suspension reason was not returned. Restoring billing/account standing or contacting Google support is required before database-backed startup can be verified. Do not delete/recreate the database as a startup workaround.
- Redis is ready at its private address, but the inspected Cloud Run services have no VPC connector. Secret Manager and Serverless VPC Access APIs are disabled.
- Eight worker services were absent; only the failed NexArr worker existed.
- The last recorded build failed in a Docker step. Old application/build stdout was no longer available in the queries used; no specific historical compiler error is inferred.

Google documents account/billing investigation for [suspended SQL instances](https://docs.cloud.google.com/sql/docs/postgres/faq#why_is_my_instance_suspended). Suspended PostgreSQL instances are subject to deletion after 90 days; resolve the suspension promptly. The date suspension began is not established here.

## Repairs in this checkout

- Workers expose `/health` on `0.0.0.0:$PORT` when PORT is supplied. Ordinary Render/background-worker execution without PORT stays supported.
- Frontend images include shared UI dependencies and knowledge-base source documents. Build-time public URLs are injected explicitly.
- Nginx sends `/api/` to the owning API, preserves the request URI, verifies upstream TLS, and secures refresh cookies. Missing assets return 404.
- One manifest builds 14 APIs, nine workers, and 17 frontends. Every image uses the Cloud Build ID, including manual builds without a commit SHA.
- The pipeline stops on a failed command, runs backend/frontend tests, pushes every image before deployment, and deploys NexArr before token-consuming products.
- Runtime settings contain discovered Cloud Run API URLs, frontend launch destinations, CORS origins, and worker job destinations. Field Companion uses NexArr.
- Passwords and signing keys are Secret Manager references, pinned to numeric versions. The pipeline no longer changes the database user's password on every deployment.
- Preflight checks stop deployment preparation when Cloud SQL, the Redis connector, or secret versions are unavailable.

These changes have not been deployed to Cloud Run. A ready revision alone does not prove database readiness, login, permissions, or cross-product functionality.

## Configure the environment

Copy `tools/gcp/deployment.example.json` to `tools/gcp/deployment.json`. The latter is ignored by Git but intentionally included in Cloud Build source uploads. Store only non-secret settings and Secret Manager **references** in that file.

Fill in:

1. A dedicated runtime service account.
2. The Cloud SQL instance connection name. Restore it to `RUNNABLE` first.
3. A ready Serverless VPC Access connector in the service region, on a network able to reach the Redis private address.
4. Existing, enabled secret versions for all 14 database URLs and shared keys.
5. Optional custom `urls` overrides keyed by Cloud Run service name.
6. Optional `serviceEnvironment` settings keyed by service name. Preserve application-specific operational settings here before redeployment: the generated environment file is authoritative.

Database URL secret values must be valid Npgsql connection strings, for example:

```text
Host=/cloudsql/PROJECT:REGION:INSTANCE;Port=5432;Database=recordarr;Username=stl;Password=SECRET_VALUE
```

Use Npgsql-compatible quoting when the password contains connection-string delimiters. Create databases/users out of band; deployments do not reset them. Use existing signing/encryption keys when migrating to Secret Manager so existing sessions and encrypted integration credentials remain readable. Rotate previously committed credentials through an explicit coordinated process.

The runtime account needs Cloud SQL Client and Secret Manager access to the exact referenced secrets. The build account needs Artifact Registry Writer, Cloud Run Admin, permission to act as the runtime account, logging permissions, and read access for preflight metadata. Determine the actual configured build account; do not assume the legacy Cloud Build account is active. Provision the Artifact Registry repository beforehand.

On an empty database, configure NexArr's `Seed__FirstAdminEmail` and related non-secret seed settings through `serviceEnvironment`, and its password through `nexarrSecrets`. Platform administration does not grant product-domain authority: configure tenant membership and StaffArr permissions through their owning workflows.

## Remaining production prerequisites

Cloud Run's writable container filesystem is ephemeral. Existing StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, and RecordArr file-storage roots require a durable storage design and migration before live document/evidence workflows. This pipeline does not claim those files are durable or silently substitute memory for storage. Validate restart/replica behavior before enabling uploads. See the [container runtime contract](https://docs.cloud.google.com/run/docs/container-contract#file_system).

Several product-named worker projects currently contain only the shared heartbeat host. Starting them does not prove product jobs execute. NexArr and Shared worker job settings and owning API work queues must be checked separately.

External providers (email/recovery, AI, external finance and other integrations) require their own configured credentials and delivery tests. No fixture mode or authentication bypass is enabled by this pipeline.

## Validate and submit

Local checks:

```powershell
python -m unittest discover -s tools/gcp -p 'test_*.py' -v
dotnet test tests/STLCompliance.Shared.Tests/STLCompliance.Shared.Tests.csproj -c Release
```

Read-only prerequisite check and manifest preparation:

```bash
python3 tools/gcp/deploy.py prepare \
  --config tools/gcp/deployment.json \
  --project stl-compliance --region us-central1 --tag preflight
```

After resolving the prerequisites, submit deliberately:

```bash
gcloud builds submit --project=stl-compliance --config=cloudbuild.yaml .
```

This creates paid build/deployment work. Worker services retain one instance with continuously allocated CPU and a maximum of one instance. They remain IAM-private; API calls still validate NexArr authentication and product authorization. Never treat Cloud Run's public API ingress as anonymous application authorization.

## Verify a rollout

Check service/revision conditions and logs, then:

- Call each API's `/health/ready` and confirm database health.
- Call `/api/v1/health` through each frontend and require JSON, not SPA HTML.
- Log in through the suite, select a tenant, and launch every ordinary product.
- Verify handoff, session refresh, logout, and Compliance Core's administrator-only studio gate.
- Run an authorized cross-product workflow and check worker/API job history.
- Verify document upload/download after a restart once durable storage is configured.

Useful read-only commands:

```bash
gcloud run services list --project=stl-compliance --region=us-central1
gcloud sql instances describe stl-compliance-v1 --project=stl-compliance
gcloud logging read 'resource.type="cloud_run_revision" AND severity>=ERROR' --project=stl-compliance --limit=30
```
