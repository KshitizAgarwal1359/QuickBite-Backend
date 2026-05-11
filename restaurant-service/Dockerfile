FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["restaurant-service/restaurant-service.csproj", "restaurant-service/"]
RUN dotnet restore "restaurant-service/restaurant-service.csproj"
COPY . .
WORKDIR "/src/restaurant-service"
RUN dotnet build "restaurant-service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "restaurant-service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "restaurant-service.dll"]
