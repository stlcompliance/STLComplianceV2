FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish workers/staffarr-worker/StaffArr.Worker/StaffArr.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StaffArr.Worker.dll"]
