FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first and restore as a distinct layer, so `docker build` only re-restores
# when a .csproj actually changes rather than on every source edit.
COPY ["src/ClinicManagement.API/ClinicManagement.API.csproj", "src/ClinicManagement.API/"]
COPY ["src/ClinicManagement.Application/ClinicManagement.Application.csproj", "src/ClinicManagement.Application/"]
COPY ["src/ClinicManagement.Infrastructure/ClinicManagement.Infrastructure.csproj", "src/ClinicManagement.Infrastructure/"]
COPY ["src/ClinicManagement.Persistence/ClinicManagement.Persistence.csproj", "src/ClinicManagement.Persistence/"]
COPY ["src/ClinicManagement.Domain/ClinicManagement.Domain.csproj", "src/ClinicManagement.Domain/"]
COPY ["src/ClinicManagement.Shared/ClinicManagement.Shared.csproj", "src/ClinicManagement.Shared/"]
RUN dotnet restore "src/ClinicManagement.API/ClinicManagement.API.csproj"

COPY . .
WORKDIR /src/src/ClinicManagement.API
RUN dotnet build "ClinicManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClinicManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Runs as non-root — the aspnet base image ships an "app" user for exactly this purpose.
USER app
ENTRYPOINT ["dotnet", "ClinicManagement.API.dll"]
