FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release

WORKDIR /src

COPY ["components/Hosting/Hosting.csproj", "components/Hosting/"]
COPY ["services/Identity/Identity.Domain/Identity.Domain.csproj", "services/Identity/Identity.Domain/"]
COPY ["services/Identity/Identity.Core/Identity.Core.csproj", "services/Identity/Identity.Core/"]
COPY ["services/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "services/Identity/Identity.Infrastructure/"]
COPY ["services/Identity/Identity.UnitTests/Identity.UnitTests.csproj", "services/Identity/Identity.UnitTests/"]
COPY ["services/Identity/Identity.FunctionalTests/Identity.FunctionalTests.csproj", "services/Identity/Identity.FunctionalTests/"]
COPY ["services/Identity/DependencyResolution.Lamar/DependencyResolution.Lamar.csproj", "services/Identity/DependencyResolution.Lamar/"]
COPY ["services/Identity/Identity.Api/Identity.Api.csproj", "services/Identity/Identity.Api/"]
COPY ["nuget.config", "services/Identity/Identity.Api/"]

RUN dotnet restore "services/Identity/Identity.Api/Identity.Api.csproj"
COPY . .
WORKDIR "/src/services/Identity/Identity.Api"

RUN ["sh", "-c", "dotnet build \"Identity.Api.csproj\" -c ${buildConfiguration} -o /app/build /property:Version=\"${buildVersion}\"" ]
FROM build AS publish
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release
RUN ["sh", "-c", "dotnet publish \"Identity.Api.csproj\" -f netcoreapp3.1 -c ${buildConfiguration} -o /app/publish"]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Laso.Identity.Api.dll"]