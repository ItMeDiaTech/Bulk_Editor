$ErrorActionPreference = "Stop"
$dir = "tools/docker"
New-Item -ItemType Directory -Force $dir | Out-Null

$files = @{
  "Dockerfile.build-linux" = @"
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /workspace
ENTRYPOINT ["dotnet"]
"@
  "Dockerfile.lint" = @"
FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN dotnet tool install -g dotnet-format
ENV PATH="/root/.dotnet/tools:${PATH}"
WORKDIR /workspace
ENTRYPOINT ["dotnet","format"]
"@
  "Dockerfile.test" = @"
FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN dotnet tool install -g dotnet-reportgenerator-globaltool
ENV PATH="/root/.dotnet/tools:${PATH}"
WORKDIR /workspace
ENTRYPOINT ["bash","-lc"]
"@
  "Dockerfile.scan-trivy" = @"
FROM aquasec/trivy:latest
ENTRYPOINT ["trivy"]
"@
  "Dockerfile.sbom-syft" = @"
FROM anchore/syft:latest
ENTRYPOINT ["syft"]
"@
  "Dockerfile.secrets-gitleaks" = @"
FROM zricethezav/gitleaks:latest
ENTRYPOINT ["gitleaks"]
"@
  "Dockerfile.dockerlint" = @"
FROM hadolint/hadolint:latest
ENTRYPOINT ["hadolint"]
"@
  "Dockerfile.diag-dotnet" = @"
FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN dotnet tool install -g dotnet-trace `
 && dotnet tool install -g dotnet-dump `
 && dotnet tool install -g dotnet-counters
ENV PATH="/root/.dotnet/tools:${PATH}"
WORKDIR /workspace
ENTRYPOINT ["bash","-lc"]
"@
}

foreach ($name in $files.Keys) {
  $p = Join-Path $dir $name
  [System.IO.File]::WriteAllText($p, $files[$name], [System.Text.UTF8Encoding]::new($false))
  Write-Host "[add] $p"
}
