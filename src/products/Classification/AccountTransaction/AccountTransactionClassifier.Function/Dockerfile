#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS installer-env
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release

WORKDIR /src

COPY ["components/DataPrivacy/DataPrivacy.csproj", "components/DataPrivacy/"]
COPY ["components/IO/IO/IO.csproj", "components/IO/IO/"]
COPY ["services/Catalog/Catalog.Domain/Catalog.Domain.csproj", "services/Catalog/Catalog.Domain/"]
COPY ["products/Classification/AccountTransaction/AccountTransactionClassifier.Function/AccountTransactionClassifier.Function.csproj", "products/Classification/AccountTransaction/AccountTransactionClassifier.Function/"]

RUN cd products/Classification/AccountTransaction/AccountTransactionClassifier.Function
RUN mkdir -p /home/site/wwwroot
RUN dotnet publish "products/Classification/AccountTransaction/AccountTransactionClassifier.Function/AccountTransactionClassifier.Function.csproj" --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:2.0-appservice 
FROM mcr.microsoft.com/azure-functions/dotnet:2.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
