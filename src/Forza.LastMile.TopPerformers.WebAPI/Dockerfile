# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy full repository context (build with context at repository root)
COPY . .

RUN dotnet restore "src/Forza.LastMile.TopPerformers.WebAPI/Forza.LastMile.TopPerformers.WebAPI.csproj"
RUN dotnet publish "src/Forza.LastMile.TopPerformers.WebAPI/Forza.LastMile.TopPerformers.WebAPI.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Forza.LastMile.TopPerformers.WebAPI.dll"]
