# STL Compliance - Google Cloud Build & Cloud Run Deployment Guide

This guide documents the production deployment pipeline for the STL Compliance / Arr Suite on **Google Cloud Platform (GCP)** using **Cloud Build**, **Artifact Registry**, **Cloud Run**, and **Cloud SQL (PostgreSQL)**.

---

## 1. Overview Architecture

- **Build Engine**: Google Cloud Build (`cloudbuild.yaml`) running on high-performance `E2_HIGHCPU_8` build instances.
- **Container Storage**: Google Artifact Registry (`us-central1-docker.pkg.dev/$PROJECT_ID/stl-compliance`).
- **Compute (Web APIs)**: 14 .NET 10 Web APIs deployed to Cloud Run (`--allow-unauthenticated`, `--port=8080`).
- **Compute (Workers)**: 9 .NET 10 Background Workers deployed to Cloud Run (`--no-allow-unauthenticated`, `--no-cpu-throttling`, `--min-instances=1`).
- **Compute (Frontends - Optional)**: Vite SPA static frontends hosted via containerized Nginx (`docker/Dockerfile.frontend`).
- **Database**: Google Cloud SQL PostgreSQL instance (`stl-compliance:us-central1:stl-compliance-v1`) attached via Cloud SQL Auth Proxy (`--add-cloudsql-instances`).

---

## 2. Deployed Services

### Web APIs (14 Services)
- `nexarr-api`
- `staffarr-api`
- `trainarr-api`
- `maintainarr-api`
- `routarr-api`
- `supplyarr-api`
- `customarr-api`
- `ordarr-api`
- `ledgarr-api`
- `reportarr-api`
- `compliancecore-api`
- `recordarr-api`
- `assurarr-api`
- `loadarr-api`

### Background Workers (9 Services)
- `nexarr-worker`
- `staffarr-worker`
- `trainarr-worker`
- `maintainarr-worker`
- `routarr-worker`
- `supplyarr-worker`
- `ledgarr-worker`
- `compliancecore-worker`
- `shared-worker`

---

## 3. GCP Prerequisites & One-Time Setup

### A. Enable Required GCP APIs
```bash
gcloud services enable \
  cloudbuild.googleapis.com \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  sqladmin.googleapis.com \
  secretmanager.googleapis.com
```

### B. Grant IAM Permissions to Cloud Build Service Account
The Cloud Build default service account (`PROJECT_NUMBER@cloudbuild.gserviceaccount.com`) requires the following roles:
- `Cloud Run Admin` (`roles/run.admin`)
- `Service Account User` (`roles/iam.serviceAccountUser`)
- `Artifact Registry Administrator` (`roles/artifactregistry.admin`)
- `Cloud SQL Client` (`roles/cloudsql.client`)

Run:
```bash
PROJECT_ID=$(gcloud config get-value project)
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format='value(projectNumber)')

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com" \
  --role="roles/run.admin"

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com" \
  --role="roles/iam.serviceAccountUser"

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com" \
  --role="roles/artifactregistry.admin"

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com" \
  --role="roles/cloudsql.client"
```

---

## 4. Triggering Deployments

### Option A: Using Helper Scripts

**On Windows (PowerShell):**
```powershell
.\scripts\deploy-gcp.ps1 -DbPass "YourSecurePass" -BootstrapSecret "YourBootstrapSecret"
```

**On Linux / macOS / Cloud Shell:**
```bash
chmod +x ./scripts/deploy-gcp.sh
GCP_DB_PASS="YourSecurePass" GCP_BOOTSTRAP_SECRET="YourBootstrapSecret" ./scripts/deploy-gcp.sh
```

### Option B: Manual gcloud build submit
```bash
gcloud builds submit \
  --config=cloudbuild.yaml \
  --substitutions=_REGION="us-central1",_ARTIFACT_REPO="stl-compliance",_CLOUDSQL_INSTANCE="stl-compliance:us-central1:stl-compliance-v1",_DB_PASS="YourSecurePass" \
  .
```

### Option C: Automated Git Triggers (CI/CD)
1. Go to **Google Cloud Console > Cloud Build > Triggers**.
2. Click **Create Trigger**.
3. Select repository provider (GitHub) and repository `tubearrteam/STLComplianceV2`.
4. Event: **Push to a branch** (`^main$`).
5. Build configuration: **Cloud Build configuration file (yaml)** -> `cloudbuild.yaml`.
6. Add substitution variables under **Advanced**:
   - `_DB_PASS`
   - `_BOOTSTRAP_SECRET`
   - `_CLOUDSQL_INSTANCE`
   - `_REGION`

---

## 5. Secret Manager Integration (Recommended for Production)

For enterprise security, store secrets in **Google Secret Manager**:

1. Create secrets:
   ```bash
   gcloud secrets create db-password --replication-policy="automatic"
   echo -n "WesRay060622!" | gcloud secrets versions add db-password --data-file=-
   ```
2. Grant secret access to the Cloud Run runtime service account:
   ```bash
   PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format='value(projectNumber)')
   gcloud secrets add-iam-policy-binding db-password \
     --member="serviceAccount:${PROJECT_NUMBER}-compute@developer.gserviceaccount.com" \
     --role="roles/secretmanager.secretAccessor"
   ```
3. Update Cloud Run services to bind `--set-secrets` instead of plain text environment variables.

---

## 6. Monitoring & Troubleshooting

### Inspect Running Cloud Run Services
```bash
gcloud run services list --region=us-central1
```

### View Service Logs
```bash
gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=nexarr-api" --limit 50
```

### Check Worker CPU Throttling
All background workers are deployed with `--no-cpu-throttling` to ensure background task queues continue executing even without inbound HTTP requests.
