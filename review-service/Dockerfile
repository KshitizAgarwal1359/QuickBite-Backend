FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["review-service/QuickBite.Review.csproj", "review-service/"]
RUN dotnet restore "review-service/QuickBite.Review.csproj"
COPY . .
WORKDIR "/src/review-service"
RUN dotnet build "QuickBite.Review.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuickBite.Review.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickBite.Review.dll"]
