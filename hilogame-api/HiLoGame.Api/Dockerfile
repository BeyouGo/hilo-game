# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + project files for a fast, cached restore
COPY HiLoGame.sln ./
COPY HiLoGame.Api/HiLoGame.Api.csproj HiLoGame.Api/
COPY HiLoGame.Application/HiLoGame.Application.csproj HiLoGame.Application/
COPY HiLoGame.Domain/HiLoGame.Domain.csproj HiLoGame.Domain/
COPY HiLoGame.Infrastructure/HiLoGame.Infrastructure.csproj HiLoGame.Infrastructure/
COPY HiLoGame.Shared/HiLoGame.Shared.csproj HiLoGame.Shared/

RUN dotnet restore HiLoGame.Api/HiLoGame.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish HiLoGame.Api/HiLoGame.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# SQLite file location (mounted in compose as a volume)
RUN mkdir -p /data

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "HiLoGame.Api.dll"]