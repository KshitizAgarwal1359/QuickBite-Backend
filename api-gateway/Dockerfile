FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["api-gateway/QuickBite.ApiGateway.csproj", "api-gateway/"]
RUN dotnet restore "api-gateway/QuickBite.ApiGateway.csproj"
COPY . .
WORKDIR "/src/api-gateway"
RUN dotnet build "QuickBite.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuickBite.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickBite.ApiGateway.dll"]
