#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Microservices/Content/Content.Api/Content.Api.csproj", "Microservices/Content/Content.Api/"]
COPY ["Microservices/Content/Content.Core/Content.Core.csproj", "Microservices/Content/Content.Core/"]
COPY ["Microservices/Content/Content.Domain/Content.Domain.csproj", "Microservices/Content/Content.Domain/"]
COPY ["Libraries/Library.Core/Library.Core.csproj", "Libraries/Library.Core/"]
COPY ["Libraries/Library.Service/Library.Service.csproj", "Libraries/Library.Service/"]
RUN dotnet restore "Microservices/Content/Content.Api/Content.Api.csproj"
COPY . .
WORKDIR "/src/Microservices/Content/Content.Api"
RUN dotnet build "Content.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Content.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Content.Api.dll"]