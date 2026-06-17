FROM node:22-bookworm-slim AS frontend-build
WORKDIR /src
ARG VITE_APP_PUBLIC_BASE_URL=https://app.stlcompliance.com
ARG VITE_SUITE_URL=https://app.stlcompliance.com
ARG VITE_STAFFARR_FRONTEND_BASE=https://staffarr.stlcompliance.com
ARG VITE_TRAINARR_FRONTEND_BASE=https://trainarr.stlcompliance.com
ARG VITE_MAINTAINARR_FRONTEND_BASE=https://maintainarr.stlcompliance.com
ARG VITE_ROUTARR_FRONTEND_BASE=https://routarr.stlcompliance.com
ARG VITE_SUPPLYARR_FRONTEND_BASE=https://supplyarr.stlcompliance.com
ARG VITE_COMPLIANCECORE_FRONTEND_BASE=https://compliancecore.stlcompliance.com
ARG VITE_COMPLIANCECORE_API_BASE=
ARG VITE_LOADARR_FRONTEND_BASE=https://loadarr.stlcompliance.com
ARG VITE_ASSURARR_FRONTEND_BASE=https://assurarr.stlcompliance.com
ARG VITE_RECORDARR_FRONTEND_BASE=https://recordarr.stlcompliance.com
ARG VITE_CUSTOMARR_FRONTEND_BASE=https://customarr.stlcompliance.com
ARG VITE_ORDARR_FRONTEND_BASE=https://ordarr.stlcompliance.com
ARG VITE_REPORTARR_FRONTEND_BASE=https://reportarr.stlcompliance.com
ARG VITE_FIELDCOMPANION_FRONTEND_BASE=https://fieldcompanion.stlcompliance.com
ENV VITE_APP_PUBLIC_BASE_URL=${VITE_APP_PUBLIC_BASE_URL} \
    VITE_SUITE_URL=${VITE_SUITE_URL} \
    VITE_STAFFARR_FRONTEND_BASE=${VITE_STAFFARR_FRONTEND_BASE} \
    VITE_TRAINARR_FRONTEND_BASE=${VITE_TRAINARR_FRONTEND_BASE} \
    VITE_MAINTAINARR_FRONTEND_BASE=${VITE_MAINTAINARR_FRONTEND_BASE} \
    VITE_ROUTARR_FRONTEND_BASE=${VITE_ROUTARR_FRONTEND_BASE} \
    VITE_SUPPLYARR_FRONTEND_BASE=${VITE_SUPPLYARR_FRONTEND_BASE} \
    VITE_COMPLIANCECORE_FRONTEND_BASE=${VITE_COMPLIANCECORE_FRONTEND_BASE} \
    VITE_COMPLIANCECORE_API_BASE=${VITE_COMPLIANCECORE_API_BASE} \
    VITE_LOADARR_FRONTEND_BASE=${VITE_LOADARR_FRONTEND_BASE} \
    VITE_ASSURARR_FRONTEND_BASE=${VITE_ASSURARR_FRONTEND_BASE} \
    VITE_RECORDARR_FRONTEND_BASE=${VITE_RECORDARR_FRONTEND_BASE} \
    VITE_CUSTOMARR_FRONTEND_BASE=${VITE_CUSTOMARR_FRONTEND_BASE} \
    VITE_ORDARR_FRONTEND_BASE=${VITE_ORDARR_FRONTEND_BASE} \
    VITE_REPORTARR_FRONTEND_BASE=${VITE_REPORTARR_FRONTEND_BASE} \
    VITE_FIELDCOMPANION_FRONTEND_BASE=${VITE_FIELDCOMPANION_FRONTEND_BASE}
COPY packages/shared-ui/package.json packages/shared-ui/package-lock.json packages/shared-ui/
RUN npm ci --prefix packages/shared-ui
COPY apps/compliancecore-frontend/package.json apps/compliancecore-frontend/package-lock.json apps/compliancecore-frontend/
RUN npm ci --prefix apps/compliancecore-frontend
COPY packages/shared-ui packages/shared-ui
COPY apps/compliancecore-frontend apps/compliancecore-frontend
RUN npm run build --prefix apps/compliancecore-frontend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish apps/compliancecore-api/ComplianceCore.Api/ComplianceCore.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-build /src/apps/compliancecore-frontend/dist ./wwwroot
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet ComplianceCore.Api.dll"]
