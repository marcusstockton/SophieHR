FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

EXPOSE 8080

USER app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SophieHR.Api/SophieHR.Api.csproj", "SophieHR.Api/"]
RUN dotnet restore "SophieHR.Api/SophieHR.Api.csproj"

COPY . .
WORKDIR "/src/SophieHR.Api"
RUN apt-get update \
    && apt-get install -y --no-install-recommends dos2unix \
    && dos2unix SophieHR.Api/wait-for-dependencies.sh || true \
    && rm -rf /var/lib/apt/lists/*

RUN dotnet publish "SophieHR.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Switch to root to copy and set permissions on the wait script, then drop back to app user
USER root
COPY --from=build /app/publish .
# Copy the wait script from the project directory (build context root). The script
# lives at SophieHR.Api/wait-for-dependencies.sh in the repo, so reference that path
# relative to the build context '.' used by docker-compose.
COPY SophieHR.Api/wait-for-dependencies.sh /wait-for-dependencies.sh
# Install dos2unix to convert CRLF to LF, install libkrb5 runtime for GSSAPI (Npgsql),
# make the script executable, and clean apt lists
RUN apt-get update \
    && apt-get install -y --no-install-recommends dos2unix libkrb5-3 libgssapi-krb5-2 \
    && dos2unix /wait-for-dependencies.sh \
    && chmod +x /wait-for-dependencies.sh \
    && rm -rf /var/lib/apt/lists/*

# Switch back to non-root user
USER app

ENTRYPOINT ["/wait-for-dependencies.sh", "dotnet", "SophieHR.Api.dll"]
