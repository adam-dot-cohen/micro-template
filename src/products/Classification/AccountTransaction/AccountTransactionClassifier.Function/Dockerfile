#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release
WORKDIR /src

COPY ["products/Classification/AccountTransaction/AccountTransactionClassifier.Function/AccountTransactionClassifier.Function.csproj", "products/Classification/AccountTransaction/AccountTransactionClassifier.Function/"]

RUN dotnet restore "products/Classification/AccountTransaction/AccountTransactionClassifier.Function/AccountTransactionClassifier.Function.csproj"
COPY . .
WORKDIR "/src/products/Classification/AccountTransaction/AccountTransactionClassifier.Function"
RUN dotnet build "AccountTransactionClassifier.Function.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountTransactionClassifier.Function.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true