docker scout quickview
docker scout cves local/dotnet-diag:latest
docker scout cves --only-fixed local/dotnet-lint:latest
docker scout compare local/dotnet-lint:latest mcr.microsoft.com/dotnet/sdk:8.0
