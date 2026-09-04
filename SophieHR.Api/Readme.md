# SophieHR

## Notes

- Uses Redis caching (currently only in `CompanyController`).
- HERE Maps API is currently used for postcode/address lookups. HERE Maps appears to have introduced changes to its pricing/payment platform, so this may need replacing in the future.
- The application uses PostgreSQL.
- PostgreSQL runs in Docker during development.
- Entity Framework Core migrations are managed from the `SophieHR.Api` project.
- When running EF commands from the host machine, PostgreSQL is exposed on port `5433`.
- When connecting from another Docker container, PostgreSQL is available using the Docker service name on port `5432`.
- EF Core design-time commands use `ApplicationDbContextFactory`.
- The Docker PostgreSQL service is named `sophiehr.db`.

---

## TODO

- [ ] Replace all DTOs with Records.
- [x] Remove AutoMapper — manual mapping is currently preferred.
- [x] Swap SQL Server for PostgreSQL.
- [ ] Investigate replacing HERE Maps for postcode/address lookups.

---

## Ideas

### Multi-tenancy

Investigate making the application multi-tenant using `CompanyId` as the tenant ID and applying it automatically to queries.

EF Core documentation:

https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy

---

# Development Environment

The application uses Docker Compose for supporting services.

Typical services include:

- SophieHR API
- PostgreSQL
- Redis
- Elasticsearch
- Kibana
- Grafana
- Prometheus
- SMTP4Dev

---

# Docker

## Check running containers

```powershell
docker compose ps
```

or:

```powershell
docker ps
```

Show container names and ports:

```powershell
docker ps --format "table {{.Names}}\t{{.Ports}}"
```

---

## Start the development environment

Start all services:

```powershell
docker compose up -d
```

Start and rebuild images:

```powershell
docker compose up -d --build
```

Start a specific service:

```powershell
docker compose up -d sophiehr.db
```

---

## Stop the development environment

Stop containers without removing them:

```powershell
docker compose stop
```

Stop and remove containers:

```powershell
docker compose down
```

Stop and remove containers and networks:

```powershell
docker compose down
```

Remove containers, networks and volumes:

> **WARNING:** This can delete your PostgreSQL data if PostgreSQL is using a Docker volume.

```powershell
docker compose down -v
```

---

## Rebuild a specific service

For example, rebuild the API:

```powershell
docker compose build sophiehr.api
```

Rebuild without using the cache:

```powershell
docker compose build --no-cache sophiehr.api
```

Rebuild and restart:

```powershell
docker compose up -d --build sophiehr.api
```

---

## View Docker logs

View all logs:

```powershell
docker compose logs
```

Follow all logs:

```powershell
docker compose logs -f
```

View API logs:

```powershell
docker compose logs sophiehr.api
```

Follow API logs:

```powershell
docker compose logs -f sophiehr.api
```

View PostgreSQL logs:

```powershell
docker compose logs sophiehr.db
```

Follow PostgreSQL logs:

```powershell
docker compose logs -f sophiehr.db
```

View the last 100 lines:

```powershell
docker compose logs --tail=100 sophiehr.db
```

---

# PostgreSQL

## PostgreSQL connection details

### From the host machine

Docker maps PostgreSQL to port `5433`:

```text
Host: localhost
Port: 5433
Database: SophieHR
Username: postgres
Password: P@55w0rd123
```

Example connection string:

```text
Host=localhost;Port=5433;Database=SophieHR;Username=postgres;Password=P@55w0rd123
```

### From another Docker container

Containers should connect to PostgreSQL using the Docker service name and PostgreSQL's internal port:

```text
Host=sophiehr.db
Port=5432
Database=SophieHR
Username=postgres
Password=P@55w0rd123
```

Example:

```text
Host=sophiehr.db;Port=5432;Database=SophieHR;Username=postgres;Password=P@55w0rd123
```

### Important

Do **not** use:

```text
Host=sophiehr.db;Port=5433
```

from inside a Docker container.

`5433` is the host-side port.

`5432` is the PostgreSQL container port.

The setup is effectively:

```text
HOST
localhost:5433
      |
      v
Docker
sophiehr.db:5432
      |
      v
PostgreSQL
```

---

# PostgreSQL Docker Commands

Check that PostgreSQL is running:

```powershell
docker compose ps sophiehr.db
```

Check PostgreSQL logs:

```powershell
docker compose logs sophiehr.db
```

Follow PostgreSQL logs:

```powershell
docker compose logs -f sophiehr.db
```

---

## Open a PostgreSQL shell inside the container

```powershell
docker compose exec sophiehr.db psql -U postgres -d SophieHR
```

Once inside `psql`:

List databases:

```sql
\l
```

List tables:

```sql
\dt
```

Describe a table:

```sql
\d "AspNetUsers"
```

Show migration history:

```sql
SELECT * FROM "__EFMigrationsHistory";
```

Exit `psql`:

```sql
\q
```

---

## Check PostgreSQL environment variables

```powershell
docker compose exec sophiehr.db env
```

Or:

```powershell
docker compose exec sophiehr.db printenv POSTGRES_USER
```

```powershell
docker compose exec sophiehr.db printenv POSTGRES_PASSWORD
```

```powershell
docker compose exec sophiehr.db printenv POSTGRES_DB
```

---

# Entity Framework Core Migrations

All EF Core commands should be run against the `SophieHR.Api` project.

From the repository root:

```powershell
cd .\SophieHR.Api
```

Alternatively, run the commands from the repository root and explicitly specify the project:

```powershell
dotnet ef ...
    --project .\SophieHR.Api\SophieHR.Api.csproj `
    --startup-project .\SophieHR.Api\SophieHR.Api.csproj
```

---

## List migrations

From the `SophieHR.Api` directory:

```powershell
dotnet ef migrations list
```

From the repository root:

```powershell
dotnet ef migrations list `
    --project .\SophieHR.Api\SophieHR.Api.csproj `
    --startup-project .\SophieHR.Api\SophieHR.Api.csproj
```

---

## Add a migration

Create a new migration:

```powershell
dotnet ef migrations add InitialCreate
```

Example:

```powershell
dotnet ef migrations add AddEmployeeFields
```

From the repository root:

```powershell
dotnet ef migrations add AddEmployeeFields `
    --project .\SophieHR.Api\SophieHR.Api.csproj `
    --startup-project .\SophieHR.Api\SophieHR.Api.csproj
```

---

## Remove the most recent migration

```powershell
dotnet ef migrations remove
```

From the repository root:

```powershell
dotnet ef migrations remove `
    --project .\SophieHR.Api\SophieHR.Api.csproj `
    --startup-project .\SophieHR.Api\SophieHR.Api.csproj
```

### Important

`migrations remove` may need to connect to the database.

The `ApplicationDbContextFactory` is responsible for creating the context during design-time operations.

If it reports:

```text
28P01: password authentication failed for user "postgres"
```

check that the connection string being used by the factory matches the actual PostgreSQL credentials.

If it reports:

```text
No such host is known
```

check whether the connection string is trying to use a Docker hostname such as:

```text
sophiehr.db
```

from the Windows host.

The Windows host should normally use:

```text
Host=localhost;Port=5433
```

---

# Check for Pending Model Changes

Check whether the EF model has changed since the last migration:

```powershell
dotnet ef migrations has-pending-model-changes
```

From the repository root:

```powershell
dotnet ef migrations has-pending-model-changes `
    --project .\SophieHR.Api\SophieHR.Api.csproj `
    --startup-project .\SophieHR.Api\SophieHR.Api.csproj
```

If EF reports:

```text
The model for context 'ApplicationDbContext' has pending changes.
```

create a migration:

```powershell
dotnet ef migrations add <MigrationName>
```

For example:

```powershell
dotnet ef migrations add AddEmployeeFields
```

Then check again:

```powershell
dotnet ef migrations has-pending-model-changes
```

---

# Apply Migrations

Apply all pending migrations:

```powershell
dotnet ef database update
```

From the repository root:

```powershell
dotnet ef database update `
    --project .\SophieHR.Api\SophieHR.Api.csproj `
    --startup-project .\SophieHR.Api\SophieHR.Api.csproj
```

---

## Apply migrations using an explicit connection string

Useful when the application configuration is pointing somewhere else:

```powershell
dotnet ef database update `
    --connection "Host=localhost;Port=5433;Database=SophieHR;Username=postgres;Password=P@55w0rd123"
```

---

## Update to a specific migration

```powershell
dotnet ef database update <MigrationName>
```

Example:

```powershell
dotnet ef database update InitialCreate
```

---

## Roll back migrations

To roll the database back to a previous migration:

```powershell
dotnet ef database update <PreviousMigrationName>
```

For example:

```powershell
dotnet ef database update InitialCreate
```

To roll the database back completely:

```powershell
dotnet ef database update 0
```

> **WARNING:** `0` means no migrations have been applied. This can result in all migration-created tables being removed.

---

# EF Migration Diagnostics

Enable verbose output:

```powershell
dotnet ef migrations list --verbose
```

For database update:

```powershell
dotnet ef database update --verbose
```

For migration removal:

```powershell
dotnet ef migrations remove --verbose
```

For pending model changes:

```powershell
dotnet ef migrations has-pending-model-changes --verbose
```

The verbose output is particularly useful for checking:

- Which `DbContext` EF is using.
- Which connection string is being used.
- Which environment is being loaded.
- Whether `ApplicationDbContextFactory` is being used.
- Which database EF is connecting to.

---

# ApplicationDbContextFactory

The project contains an `IDesignTimeDbContextFactory<ApplicationDbContext>`.

This is used by EF Core when running commands such as:

```powershell
dotnet ef migrations add
dotnet ef migrations remove
dotnet ef database update
dotnet ef migrations list
dotnet ef migrations has-pending-model-changes
```

The factory should use a connection string that is accessible from the machine running the EF command.

For example, when running EF commands from Windows:

```text
Host=localhost;Port=5433;Database=SophieHR;Username=postgres;Password=P@55w0rd123
```

Do not use the Docker service name:

```text
Host=sophiehr.db
```

when the EF command is running on the Windows host.

---

# Application vs Docker Connection Strings

There are effectively two environments.

## Running the API inside Docker

Use:

```text
Host=sophiehr.db;Port=5432
```

Example:

```text
Host=sophiehr.db;Port=5432;Database=SophieHR;Username=postgres;Password=P@55w0rd123
```

This is normally supplied by Docker Compose:

```yaml
environment:
  ConnectionStrings__DefaultConnection: "Host=sophiehr.db;Port=5432;Database=SophieHR;Username=postgres;Password=${POSTGRES_PASSWORD}"
```

---

## Running the API directly from Visual Studio / Windows

Use:

```text
Host=localhost;Port=5433
```

Example:

```text
Host=localhost;Port=5433;Database=SophieHR;Username=postgres;Password=P@55w0rd123
```

The Docker port mapping is:

```yaml
ports:
  - "5433:5432"
```

This means:

```text
Windows localhost:5433
        |
        v
Docker PostgreSQL:5432
```

---

# Common PostgreSQL / Docker Problems

## `Connection refused`

Example:

```text
Failed to connect to 172.18.0.3:5433
Connection refused
```

If the connection is using a Docker IP and port `5433`, the port is probably wrong.

Inside Docker, PostgreSQL uses:

```text
sophiehr.db:5432
```

not:

```text
sophiehr.db:5433
```

---

## `No such host is known`

Example:

```text
No such host is known. (sophiehr.db:5432)
```

This usually means the command is running on the Windows host but is trying to use the Docker service name.

Use:

```text
Host=localhost;Port=5433
```

instead.

---

## `password authentication failed`

Example:

```text
28P01: password authentication failed for user "postgres"
```

Check:

1. The username.
2. The password.
3. The database.
4. The connection string being used.
5. Whether PostgreSQL was originally initialised with a different password.

Check the Docker Compose environment:

```powershell
docker compose config
```

Look at the PostgreSQL service configuration.

You can also inspect the environment inside the running container:

```powershell
docker compose exec sophiehr.db printenv POSTGRES_PASSWORD
```

### Important

Changing:

```yaml
POSTGRES_PASSWORD:
```

in Docker Compose does **not necessarily change the password of an already-initialised PostgreSQL database**.

The `POSTGRES_*` environment variables are primarily used when PostgreSQL initialises its data directory.

If the database volume already exists, changing the environment variable may not change the existing PostgreSQL user's password.

---

# Check Which Process Is Using PostgreSQL Port 5432

On Windows:

```powershell
Get-NetTCPConnection -LocalPort 5432 -State Listen |
    Select-Object LocalAddress, LocalPort, OwningProcess
```

Find the process:

```powershell
Get-Process -Id <ProcessId>
```

For example:

```powershell
Get-Process -Id 7248
```

Check whether Docker is exposing the port:

```powershell
docker ps --format "table {{.Names}}\t{{.Ports}}"
```

Expected Docker mapping:

```text
0.0.0.0:5433->5432/tcp
```

If another local PostgreSQL installation is listening on port `5432`, this can cause confusion.

---

# Resetting the PostgreSQL Development Database

If the database is purely development data and can safely be destroyed, the simplest reset is:

```powershell
docker compose down -v
```

Then:

```powershell
docker compose up -d
```

This recreates the PostgreSQL container and its volume.

> **WARNING:** This deletes the Docker volumes and therefore destroys the development database data.

After recreating the database:

```powershell
dotnet ef database update
```

---

# Completely Rebuild the Docker Environment

If the development environment gets into a particularly strange state:

```powershell
docker compose down -v
docker compose build --no-cache
docker compose up -d
```

Then check:

```powershell
docker compose ps
```

And logs:

```powershell
docker compose logs -f
```

> **WARNING:** `down -v` deletes Docker volumes.

---

# Useful Docker Inspection Commands

Show the fully resolved Compose configuration:

```powershell
docker compose config
```

This is particularly useful for checking:

- Environment variables.
- Port mappings.
- Service names.
- Volumes.
- Networks.

Show a container's environment:

```powershell
docker inspect sophiehr-sophiehr.db-1
```

Show a container's network configuration:

```powershell
docker inspect sophiehr-sophiehr.db-1
```

Show Docker networks:

```powershell
docker network ls
```

Inspect the Compose network:

```powershell
docker network inspect sophiehr_default
```

---

# Test PostgreSQL Connectivity From the API Container

If the API is running in Docker, open a shell in the API container:

```powershell
docker compose exec sophiehr.api sh
```

Depending on the image, `bash` may be available instead:

```powershell
docker compose exec sophiehr.api bash
```

The API should be able to resolve:

```text
sophiehr.db
```

and PostgreSQL should be available on:

```text
5432
```

---

# Useful EF Core Workflow

For normal development, the typical workflow is:

## 1. Change the EF model

Modify an entity or `ApplicationDbContext`.

---

## 2. Check for pending model changes

```powershell
dotnet ef migrations has-pending-model-changes
```

---

## 3. Create a migration

```powershell
dotnet ef migrations add <DescriptiveMigrationName>
```

Example:

```powershell
dotnet ef migrations add AddEmployeeAvatar
```

---

## 4. Review the generated migration

Check:

```text
SophieHR.Api/Migrations/
```

Make sure EF has generated what was expected.

---

## 5. Apply the migration

```powershell
dotnet ef database update
```

---

## 6. Verify migrations

```powershell
dotnet ef migrations list
```

Or check PostgreSQL directly:

```powershell
docker compose exec sophiehr.db psql -U postgres -d SophieHR
```

Then:

```sql
SELECT * FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
```

---

# Starting From a Completely Fresh Database

If migrations have been deleted/recreated or the database is in an inconsistent development state:

```powershell
docker compose down -v
docker compose up -d
```

Then create/apply the initial migration:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Check:

```powershell
dotnet ef migrations list
```

---

# Migration Gotchas

EF Core migrations and the PostgreSQL database should be treated as two separate things.

The migration files live in:

```text
SophieHR.Api/Migrations/
```

The migration history lives inside PostgreSQL:

```text
__EFMigrationsHistory
```

EF compares the current model against the migration snapshot when determining whether model changes are pending.

If migrations are manually deleted, the database's `__EFMigrationsHistory` table may still contain records referring to migrations that no longer exist in the project.

If migrations have been completely reset during development, it may be easier to recreate the development database:

```powershell
docker compose down -v
docker compose up -d
```

Then create a new initial migration:

```powershell
dotnet ef migrations add InitialCreate
```

and apply it:

```powershell
dotnet ef database update
```

---

# Useful EF Commands — Quick Reference

```powershell
# List migrations
dotnet ef migrations list

# Add migration
dotnet ef migrations add <MigrationName>

# Remove latest migration
dotnet ef migrations remove

# Check for pending model changes
dotnet ef migrations has-pending-model-changes

# Update database
dotnet ef database update

# Update to a specific migration
dotnet ef database update <MigrationName>

# Roll database back completely
dotnet ef database update 0

# Verbose output
dotnet ef migrations list --verbose
dotnet ef migrations remove --verbose
dotnet ef database update --verbose
dotnet ef migrations has-pending-model-changes --verbose
```

---

# Useful Docker Commands — Quick Reference

```powershell
# Start everything
docker compose up -d

# Start and rebuild
docker compose up -d --build

# Stop everything
docker compose stop

# Stop and remove containers
docker compose down

# Stop, remove containers and volumes
docker compose down -v

# Show running containers
docker compose ps

# Show logs
docker compose logs

# Follow logs
docker compose logs -f

# API logs
docker compose logs -f sophiehr.api

# PostgreSQL logs
docker compose logs -f sophiehr.db

# Rebuild API
docker compose build sophiehr.api

# Rebuild API without cache
docker compose build --no-cache sophiehr.api

# Show resolved Compose configuration
docker compose config

# Open PostgreSQL shell
docker compose exec sophiehr.db psql -U postgres -d SophieHR
```

---

# Connection String Quick Reference

| Where the application is running | Host | Port |
|---|---|---:|
| Windows / Visual Studio | `localhost` | `5433` |
| EF Core CLI on Windows | `localhost` | `5433` |
| API container | `sophiehr.db` | `5432` |
| Any container on the same Compose network | `sophiehr.db` | `5432` |

### Host machine

```text
Host=localhost;Port=5433;Database=SophieHR;Username=postgres;Password=P@55w0rd123
```

### Docker

```text
Host=sophiehr.db;Port=5432;Database=SophieHR;Username=postgres;Password=P@55w0rd123
```

---

# Current Docker PostgreSQL Port Mapping

```yaml
ports:
  - "5433:5432"
```

Meaning:

```text
                 Docker
                 ┌───────────────────────┐
Windows          │                       │
localhost:5433 ───────► PostgreSQL:5432 │
                 │                       │
                 └───────────────────────┘
```

Therefore:

- `localhost:5433` = PostgreSQL from Windows.
- `sophiehr.db:5432` = PostgreSQL from Docker containers.
- `sophiehr.db:5433` = **incorrect**.
- `localhost:5432` = potentially another PostgreSQL instance on Windows, so avoid using it unless intentionally configured that way.

---

# Troubleshooting Checklist

When EF Core gives a connection error, first determine **where EF is running**.

### EF running from Windows

Use:

```text
Host=localhost;Port=5433
```

### Application running inside Docker

Use:

```text
Host=sophiehr.db;Port=5432
```

Then check:

```powershell
docker compose ps
```

Confirm PostgreSQL is healthy.

Check:

```powershell
docker compose logs sophiehr.db
```

Check the resolved Compose configuration:

```powershell
docker compose config
```

Check which port Docker exposes:

```powershell
docker ps --format "table {{.Names}}\t{{.Ports}}"
```

If authentication fails:

```text
28P01: password authentication failed
```

verify the actual credentials inside the running PostgreSQL container rather than relying only on `.env` or `docker-compose.yml`.

If the database is disposable development data and configuration has become inconsistent:

```powershell
docker compose down -v
docker compose up -d
```

Then:

```powershell
dotnet ef database update
```

---

# Recommended Migration Workflow

For this project, the safest normal workflow is:

```powershell
# 1. Make code/model changes

# 2. Check whether EF sees model changes
dotnet ef migrations has-pending-model-changes

# 3. Create a migration
dotnet ef migrations add <DescriptiveName>

# 4. Review the generated migration

# 5. Apply it
dotnet ef database update

# 6. Verify migrations
dotnet ef migrations list
```

If something goes wrong, use verbose output:

```powershell
dotnet ef database update --verbose
```

Pay particular attention to the connection shown in the output.

For commands running on Windows, it should normally show something equivalent to:

```text
localhost:5433
```

not:

```text
sophiehr.db:5433
```

and not:

```text
localhost:5432
```

---

# Development Database Reset

For a clean development reset:

```powershell
docker compose down -v
docker compose up -d
dotnet ef database update
```

This should leave the database recreated from the current migrations.