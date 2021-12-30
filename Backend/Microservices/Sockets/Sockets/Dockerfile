#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Microservices/Sockets/Sockets/Sockets.csproj", "Microservices/Sockets/Sockets/"]
COPY ["Libraries/Library.Core/Library.Core.csproj", "Libraries/Library.Core/"]
COPY ["Libraries/Library.Service/Library.Service.csproj", "Libraries/Library.Service/"]
RUN dotnet restore "Microservices/Sockets/Sockets/Sockets.csproj"
COPY . .
WORKDIR "/src/Microservices/Sockets"
RUN dotnet build "Sockets.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sockets.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sockets.dll"]
