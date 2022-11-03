#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# CLI:
# docker build -t iamrobert/sc-api-gateway:v0.0.x-alpha -f Gateway/Dockerfile .
# docker push iamrobert/sc-api-gateway:v0.0.x-alpha

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Gateway/Gateway.csproj", "Gateway/"]
COPY ["Libraries/Library.Service/Library.Service.csproj", "Libraries/Library.Service/"]
COPY ["Libraries/Library.Core/Library.Core.csproj", "Libraries/Library.Core/"]
RUN dotnet restore "Gateway/Gateway.csproj"
COPY . .
WORKDIR "/src/Gateway"
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]