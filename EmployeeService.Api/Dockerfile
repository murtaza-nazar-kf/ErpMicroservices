FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EmployeeService.Api/EmployeeService.Api.csproj", "EmployeeService.Api/"]
COPY ["EmployeeService.Application/EmployeeService.Application.csproj", "EmployeeService.Application/"]
COPY ["EmployeeService.Domain/EmployeeService.Domain.csproj", "EmployeeService.Domain/"]
COPY ["EmployeeService.Infrastructure/EmployeeService.Infrastructure.csproj", "EmployeeService.Infrastructure/"]

RUN dotnet restore "./EmployeeService.Api/EmployeeService.Api.csproj" --no-cache

ARG BUILD_CONFIGURATION=Release

COPY . .

WORKDIR "/src/EmployeeService.Api"
RUN dotnet publish "./EmployeeService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "EmployeeService.Api.dll"]
