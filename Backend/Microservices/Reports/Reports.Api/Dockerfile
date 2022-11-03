#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Microservices/Reports/Reports.Api/Reports.Api.csproj", "Microservices/Reports/Reports.Api/"]
COPY ["Microservices/Reports/Reports.Core/Reports.Core.csproj", "Microservices/Reports/Reports.Core/"]
COPY ["Microservices/Reports/Reports.Domain/Reports.Domain.csproj", "Microservices/Reports/Reports.Domain/"]
COPY ["Libraries/Library.Core/Library.Core.csproj", "Libraries/Library.Core/"]
COPY ["Libraries/Library.Service/Library.Service.csproj", "Libraries/Library.Service/"]
RUN dotnet restore "Microservices/Reports/Reports.Api/Reports.Api.csproj"
COPY . .
WORKDIR "/src/Microservices/Reports/Reports.Api"
RUN dotnet build "Reports.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Reports.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reports.Api.dll"]