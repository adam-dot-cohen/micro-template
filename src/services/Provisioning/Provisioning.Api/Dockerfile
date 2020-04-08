#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release
WORKDIR /src

COPY ["services/Provisioning/Provisioning.Core/Provisioning.Core.csproj", "services/Provisioning/Provisioning.Core/"]
COPY ["services/Provisioning/Provisioning.UnitTests/Provisioning.UnitTests.csproj", "services/Provisioning/Provisioning.UnitTests/"]
COPY ["services/Provisioning/Provisioning.Api/Provisioning.Api.csproj", "services/Provisioning/Provisioning.Api/"]
COPY ["nuget.config", "services/Provisioning/Provisioning.Api/"]

RUN dotnet restore "services/Provisioning/Provisioning.Api/Provisioning.Api.csproj"
COPY . .
WORKDIR "/src/services/Provisioning/Provisioning.Api"
RUN ["sh", "-c", "dotnet build \"Provisioning.Api.csproj\" -c ${buildConfiguration} -o /app/build /property:Version=\"${buildVersion}\""]

FROM build AS publish
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release
RUN ["sh", "-c", "dotnet publish \"Provisioning.Api.csproj\" -framework netcoreapp3.1 -c ${buildConfiguration} -o /app/publish"]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Laso.Provisioning.Api.dll"]