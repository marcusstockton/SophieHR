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

RUN dotnet publish "SophieHR.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SophieHR.Api.dll"]