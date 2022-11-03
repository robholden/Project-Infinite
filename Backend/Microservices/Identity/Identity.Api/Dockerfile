#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# CLI:
# docker build -t iamrobert/sc-api-identity:latest -f Microservices/Identity/Identity.Api/Dockerfile .
# docker push iamrobert/sc-api-identity:latest

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Microservices/Identity/Identity.Api/Identity.Api.csproj", "Microservices/Identity/Identity.Api/"]
COPY ["Microservices/Identity/Identity.Core/Identity.Core.csproj", "Microservices/Identity/Identity.Core/"]
COPY ["Microservices/Identity/Identity.Domain/Identity.Domain.csproj", "Microservices/Identity/Identity.Domain/"]
COPY ["Libraries/Library.Core/Library.Core.csproj", "Libraries/Library.Core/"]
COPY ["Libraries/Library.Service/Library.Service.csproj", "Libraries/Library.Service/"]
RUN dotnet restore "Microservices/Identity/Identity.Api/Identity.Api.csproj"
COPY . .
WORKDIR "/src/Microservices/Identity/Identity.Api"
RUN dotnet build "Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]