FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["menu-service/menu-service.csproj", "menu-service/"]
RUN dotnet restore "menu-service/menu-service.csproj"
COPY . .
WORKDIR "/src/menu-service"
RUN dotnet build "menu-service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "menu-service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "menu-service.dll"]
