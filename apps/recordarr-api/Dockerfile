FROM node:22-bookworm-slim AS frontend-build
WORKDIR /src
ARG VITE_APP_PUBLIC_BASE_URL=https://app.stlcompliance.com
ARG VITE_ROUTER_BASENAME=/recordarr
ARG VITE_SUITE_URL=https://app.stlcompliance.com
ARG VITE_STAFFARR_FRONTEND_BASE=https://app.stlcompliance.com/staffarr
ARG VITE_TRAINARR_FRONTEND_BASE=https://app.stlcompliance.com/trainarr
ARG VITE_MAINTAINARR_FRONTEND_BASE=https://app.stlcompliance.com/maintainarr
ARG VITE_ROUTARR_FRONTEND_BASE=https://app.stlcompliance.com/routarr
ARG VITE_SUPPLYARR_FRONTEND_BASE=https://app.stlcompliance.com/supplyarr
ARG VITE_COMPLIANCECORE_FRONTEND_BASE=https://app.stlcompliance.com/compliancecore
ARG VITE_LOADARR_FRONTEND_BASE=https://app.stlcompliance.com/loadarr
ARG VITE_RECORDARR_FRONTEND_BASE=https://app.stlcompliance.com/recordarr
ARG VITE_RECORDARR_API_BASE=https://recordarr-api.onrender.com
ARG VITE_COMPANION_FRONTEND_BASE=https://fieldcompanion-frontend-vf7o.onrender.com
ENV VITE_APP_PUBLIC_BASE_URL=${VITE_APP_PUBLIC_BASE_URL} \
    VITE_ROUTER_BASENAME=${VITE_ROUTER_BASENAME} \
    VITE_SUITE_URL=${VITE_SUITE_URL} \
    VITE_STAFFARR_FRONTEND_BASE=${VITE_STAFFARR_FRONTEND_BASE} \
    VITE_TRAINARR_FRONTEND_BASE=${VITE_TRAINARR_FRONTEND_BASE} \
    VITE_MAINTAINARR_FRONTEND_BASE=${VITE_MAINTAINARR_FRONTEND_BASE} \
    VITE_ROUTARR_FRONTEND_BASE=${VITE_ROUTARR_FRONTEND_BASE} \
    VITE_SUPPLYARR_FRONTEND_BASE=${VITE_SUPPLYARR_FRONTEND_BASE} \
    VITE_COMPLIANCECORE_FRONTEND_BASE=${VITE_COMPLIANCECORE_FRONTEND_BASE} \
    VITE_LOADARR_FRONTEND_BASE=${VITE_LOADARR_FRONTEND_BASE} \
    VITE_RECORDARR_FRONTEND_BASE=${VITE_RECORDARR_FRONTEND_BASE} \
    VITE_RECORDARR_API_BASE=${VITE_RECORDARR_API_BASE} \
    VITE_COMPANION_FRONTEND_BASE=${VITE_COMPANION_FRONTEND_BASE}
COPY packages/shared-ui/package.json packages/shared-ui/package-lock.json packages/shared-ui/
RUN npm ci --prefix packages/shared-ui
COPY apps/recordarr-frontend/package.json apps/recordarr-frontend/package-lock.json apps/recordarr-frontend/
RUN npm ci --prefix apps/recordarr-frontend
COPY packages/shared-ui packages/shared-ui
COPY apps/recordarr-frontend apps/recordarr-frontend
RUN npm run build --prefix apps/recordarr-frontend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish apps/recordarr-api/RecordArr.Api/RecordArr.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-build /src/apps/recordarr-frontend/dist ./wwwroot
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet RecordArr.Api.dll"]
