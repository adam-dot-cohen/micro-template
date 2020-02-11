#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["services/Identity/Identity.Domain/Identity.Domain.csproj", "services/Identity/Identity.Domain/"]
COPY ["services/Identity/Identity.Core/Identity.Core.csproj", "services/Identity/Identity.Core/"]
COPY ["services/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "services/Identity/Identity.Infrastructure/"]
COPY ["services/Identity/Identity.UnitTests/Identity.UnitTests.csproj", "services/Identity/Identity.UnitTests/"]
COPY ["services/Identity/Identity.FunctionalTests/Identity.FunctionalTests.csproj", "services/Identity/Identity.FunctionalTests/"]
COPY ["services/Identity/Identity.Api/Identity.Api.csproj", "services/Identity/Identity.Api/"]

RUN dotnet restore "services/Identity/Identity.Api/Identity.Api.csproj"
COPY . .
WORKDIR "/src/services/Identity/Identity.Api"
RUN dotnet build "Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.Api.csproj" --framework netcoreapp3.1 -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]