#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:6.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release
WORKDIR /src

COPY ["services/Triggers/Triggers.csproj", "services/Triggers/"]

RUN dotnet restore "services/Triggers/Triggers.csproj"
COPY . .
WORKDIR "/src/services/Triggers"
RUN dotnet build "Triggers.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Triggers.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

