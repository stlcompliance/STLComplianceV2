# Google Cloud Run deployment and recovery

The repository's default deployment remains Render. This is the separate, opt-in Google Cloud Run pipeline. Local edits do not change either deployed environment.

## Investigation — October 10, 2026

Read-only inspection of project `stl-compliance`, region `us-central1`, found:

- `routarr-api` and `nexarr-worker` have failed startup revisions. The worker host previously had no HTTP listener at all. Retained RoutArr logs contain the platform startup failure, but no application exception proving its original cause.
- `stlcompliancesite` and `stlcompliancekb` point to image tags absent from Artifact Registry.
- The suite frontend returns HTTP 200 **HTML** for `/api/v1/health`. The former Nginx image had no API proxy.
- Cloud SQL `stl-compliance-v1` is **SUSPENDED**, with activation policy `ALWAYS`. The owner confirmed that its trial expired. The subsequent upgrade failed with `ERROR_STOCKOUT`. The owner authorized a fresh replacement because the stored data is negligible; the original instance remains untouched.
- Redis is ready at its private address, but the inspected Cloud Run services have no VPC connector. Secret Manager and Serverless VPC Access APIs are disabled.
- Eight worker services were absent; only the failed NexArr worker existed.
- The last recorded build failed in a Docker step. Old application/build stdout was no longer available in the queries used; no specific historical compiler error is inferred.

The replacement `stl-compliance-v2` is **RUNNABLE** in `us-central1-b`: PostgreSQL 18, Enterprise, 2 vCPU / 8 GiB, 20 GB SSD, daily backups, point-in-time recovery, and deletion protection. It is a fresh database, not a migration of the suspended instance. Google documents the [Cloud SQL trial lifecycle](https://docs.cloud.google.com/sql/docs/postgres/free-trial-instance); preservation of the old suspended instance remains subject to that lifecycle.

Secret Manager is now enabled, and the dedicated runtime identity is `stl-cloud-run@stl-compliance.iam.gserviceaccount.com`. Direct VPC egress uses the existing `default` network/subnet to reach Redis; a paid connector is not required.

Provisioning completed for all 14 product databases and separate database users, pinned database/signing/bootstrap secrets, and six private document buckets. Bucket public-access prevention is enforced and uniform access is enabled. The runtime identity has Cloud SQL Client and scoped secret/bucket access. Read-only deployment preflight succeeded and generated the 40-service manifest. These infrastructure checks are not application migration, login, Redis connectivity, or end-to-end workflow tests.

## Repairs in this checkout

- Workers expose `/health` on `0.0.0.0:$PORT` when PORT is supplied. Ordinary Render/background-worker execution without PORT stays supported.
- Frontend images include shared UI dependencies and knowledge-base source documents. Build-time public URLs are injected explicitly.
- Nginx sends `/api/` to the owning API, preserves the request URI, verifies upstream TLS, and secures refresh cookies. Missing assets return 404.
- One manifest builds 14 APIs, nine workers, and 17 frontends. Every image uses the Cloud Build ID, including manual builds without a commit SHA.
- The pipeline stops on a failed command, runs backend/frontend tests, pushes every image before deployment, and deploys NexArr before token-consuming products.
- Runtime settings contain discovered Cloud Run API URLs, frontend launch destinations, CORS origins, and worker job destinations. Field Companion uses NexArr.
- Passwords and signing keys are Secret Manager references, pinned to numeric versions. The pipeline no longer changes the database user's password on every deployment.
- Preflight checks stop deployment preparation when Cloud SQL, the Redis connector or Direct VPC subnet, or secret versions are unavailable.
- Optional per-product private Cloud Storage buckets mount at `/mnt/documents`, with each existing owning service's storage setting redirected there. No cross-product database joins or file-ownership changes are introduced.

These changes have not been deployed to Cloud Run. A ready revision alone does not prove database readiness, login, permissions, or cross-product functionality.

### Release gate status

The owner subsequently approved the scoped temporary [Render-only exception](docs/operations/GCP_RELEASE_EXCEPTION_2026-10-11.md). Cloud Build `f4d743fd-f33f-4f33-9eaf-a8d25c6ef1fc` passed infrastructure preflight with that exception, but reported eight health-test failures and four OpenAPI snapshot mismatches. It was cancelled before image builds or deployment. These application-test failures are not exempted; no Cloud Run cutover occurred.

The focused CI contract run passed 21 tests and failed two pre-existing checks because these files are absent:

- `docs/operations/RENDER_STAGING_SHIP_GATE_V1.md`
- `docs/deployment/ENV_VARS_V1.md`

The broader backend run was stopped after that independent gate failed; it is not a passing full-suite result. Do not bypass the failures, submit a knowingly failing paid build, or redirect live services to the fresh databases until the release gate is repaired and rerun. The new databases remain empty until application migrations and bootstrap execute during a successful gated rollout. No existing Cloud Run service has been cut over.

The Field Companion time-dependent fixture is now deterministic and its 154 tests pass. Deployment/bootstrap tests (15), frontend security tests (4), and YAML/shell syntax checks pass. Earlier validation built all 17 frontends and the Release solution; Docker container execution remains unverified locally because the Docker daemon is unavailable.

## Configure the environment

Copy `tools/gcp/deployment.example.json` to `tools/gcp/deployment.json`. The latter is ignored by Git but intentionally included in Cloud Build source uploads. Store only non-secret settings and Secret Manager **references** in that file.

Fill in:

1. A dedicated runtime service account.
2. The Cloud SQL instance connection name. Restore it to `RUNNABLE` first.
3. A Direct VPC `network` and `subnet`, or a ready `vpcConnector`, able to reach the Redis private address.
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

For a deliberately fresh replacement, `tools/gcp/bootstrap.py` creates separate product databases/users, generates pinned secrets without writing secret values to disk, and writes the ignored deployment configuration. It reuses existing secret versions on retry and does not reset existing database-user passwords. Its `--instance` argument must name the replacement, never the suspended source. Cloud SQL-created database users initially receive Google's administrative database role; review and reduce database privileges before production release. The bootstrap administrator is `tseders93@gmail.com`; its generated password is stored only in Secret Manager as `stl-compliance-v2-admin-password`. The account itself does not exist until NexArr bootstrap runs.

```powershell
python tools/gcp/bootstrap.py --project stl-compliance --instance stl-compliance-v2 --runtime-account stl-cloud-run@stl-compliance.iam.gserviceaccount.com --admin-email tseders93@gmail.com --redis-url 10.66.185.43:6379
python tools/gcp/storage.py --project stl-compliance --config tools/gcp/deployment.json
```

## Remaining production prerequisites

Cloud Run's writable container filesystem is ephemeral. `tools/gcp/storage.py` provisions private per-product buckets and configures the six existing file-storage roots. Run it against the deployment configuration before rollout. Mounts disable metadata caching to avoid stale replica reads; they do not implement application locking, quarantine, scanning, retention, or legal-hold enforcement. Validate upload/download, restart and concurrent-write behavior before declaring document/evidence workflows production-ready. See [Cloud Storage mount limitations](https://docs.cloud.google.com/run/docs/configuring/services/cloud-storage-volume-mounts).

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
