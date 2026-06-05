FROM node:22-bookworm-slim AS frontend-build
WORKDIR /src
ARG VITE_APP_PUBLIC_BASE_URL=https://app.stlcompliance.com
ARG VITE_SUITE_URL=https://app.stlcompliance.com
ARG VITE_STAFFARR_FRONTEND_BASE=https://staffarr-api-oyrj.onrender.com
ARG VITE_TRAINARR_FRONTEND_BASE=https://trainarr-api-nsiw.onrender.com
ARG VITE_MAINTAINARR_FRONTEND_BASE=https://maintainarr-api-z39r.onrender.com
ARG VITE_ROUTARR_FRONTEND_BASE=https://routarr-api-l9uj.onrender.com
ARG VITE_SUPPLYARR_FRONTEND_BASE=https://supplyarr-api-dn8u.onrender.com
ARG VITE_COMPLIANCECORE_FRONTEND_BASE=https://compliancecore-api-w7e5.onrender.com
ARG VITE_LOADARR_FRONTEND_BASE=https://loadarr-api.onrender.com
ARG VITE_LOADARR_API_BASE=https://loadarr-api.onrender.com
ARG VITE_COMPANION_FRONTEND_BASE=https://companion-frontend-vf7o.onrender.com
ENV VITE_APP_PUBLIC_BASE_URL=${VITE_APP_PUBLIC_BASE_URL} \
    VITE_SUITE_URL=${VITE_SUITE_URL} \
    VITE_STAFFARR_FRONTEND_BASE=${VITE_STAFFARR_FRONTEND_BASE} \
    VITE_TRAINARR_FRONTEND_BASE=${VITE_TRAINARR_FRONTEND_BASE} \
    VITE_MAINTAINARR_FRONTEND_BASE=${VITE_MAINTAINARR_FRONTEND_BASE} \
    VITE_ROUTARR_FRONTEND_BASE=${VITE_ROUTARR_FRONTEND_BASE} \
    VITE_SUPPLYARR_FRONTEND_BASE=${VITE_SUPPLYARR_FRONTEND_BASE} \
    VITE_COMPLIANCECORE_FRONTEND_BASE=${VITE_COMPLIANCECORE_FRONTEND_BASE} \
    VITE_LOADARR_FRONTEND_BASE=${VITE_LOADARR_FRONTEND_BASE} \
    VITE_LOADARR_API_BASE=${VITE_LOADARR_API_BASE} \
    VITE_COMPANION_FRONTEND_BASE=${VITE_COMPANION_FRONTEND_BASE}
COPY packages/shared-ui/package.json packages/shared-ui/package-lock.json packages/shared-ui/
RUN npm ci --prefix packages/shared-ui
COPY apps/loadarr-frontend/package.json apps/loadarr-frontend/package-lock.json apps/loadarr-frontend/
RUN npm ci --prefix apps/loadarr-frontend
COPY packages/shared-ui packages/shared-ui
COPY apps/loadarr-frontend apps/loadarr-frontend
RUN npm run build --prefix apps/loadarr-frontend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish apps/loadarr-api/LoadArr.Api/LoadArr.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-build /src/apps/loadarr-frontend/dist ./wwwroot
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet LoadArr.Api.dll"]
