#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Microservices/Comms/Comms.Api/Comms.Api.csproj", "Microservices/Comms/Comms.Api/"]
COPY ["Microservices/Comms/Comms.Core/Comms.Core.csproj", "Microservices/Comms/Comms.Core/"]
COPY ["Microservices/Comms/Comms.Domain/Comms.Domain.csproj", "Microservices/Comms/Comms.Domain/"]
COPY ["Libraries/Library.Core/Library.Core.csproj", "Libraries/Library.Core/"]
COPY ["Libraries/Library.Service/Library.Service.csproj", "Libraries/Library.Service/"]
RUN dotnet restore "Microservices/Comms/Comms.Api/Comms.Api.csproj"
COPY . .
WORKDIR "/src/Microservices/Comms/Comms.Api"
RUN dotnet build "Comms.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Comms.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Comms.Api.dll"]