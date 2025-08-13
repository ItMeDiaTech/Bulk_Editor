param(
  [ValidateSet("build-all","lint","test","scan","sbom","secrets","dockerlint","diag-shell")]
  [string]$task = "build-all"
)
$ErrorActionPreference = "Stop"

function Assert-Docker {
  try { docker info | Out-Null } catch { Write-Error "Docker Desktop isn't running."; exit 1 }
}

function Img($df,$tag){
  Write-Host "[build] tools/docker/$df  ->  $tag"
  docker build -f "tools/docker/$df" -t $tag .
  if ($LASTEXITCODE -ne 0) { throw "Build failed: $df" }
}

switch ($task) {
  "build-all" {
    Assert-Docker
    $needed = @(
      "Dockerfile.build-linux",
      "Dockerfile.lint",
      "Dockerfile.test",
      "Dockerfile.scan-trivy",
      "Dockerfile.sbom-syft",
      "Dockerfile.secrets-gitleaks",
      "Dockerfile.dockerlint",
      "Dockerfile.diag-dotnet"
    )
    $missing = $needed | Where-Object { -not (Test-Path "tools/docker/$_") }
    if ($missing) { throw "Missing Dockerfiles:`n$($missing -join "`n")" }

    Img "Dockerfile.build-linux"      "local/dotnet-build:latest"
    Img "Dockerfile.lint"             "local/dotnet-lint:latest"
    Img "Dockerfile.test"             "local/dotnet-test:latest"
    Img "Dockerfile.scan-trivy"       "local/scan-trivy:latest"
    Img "Dockerfile.sbom-syft"        "local/sbom-syft:latest"
    Img "Dockerfile.secrets-gitleaks" "local/secrets-gitleaks:latest"
    Img "Dockerfile.dockerlint"       "local/dockerlint:latest"
    Img "Dockerfile.diag-dotnet"      "local/dotnet-diag:latest"
    Write-Host "Built all images."
  }

  "lint"       { Assert-Docker; docker run --rm -v "${PWD}:/workspace" -w /workspace local/dotnet-lint:latest }
  "test"       { Assert-Docker; docker run --rm -v "${PWD}:/workspace" -w /workspace local/dotnet-test:latest 'dotnet test --logger trx --collect:"XPlat Code Coverage" && reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage' }
  "scan"       { Assert-Docker; docker run --rm -v "${PWD}:/workspace" -w /workspace local/scan-trivy:latest fs --severity HIGH,CRITICAL . }
  "sbom"       { Assert-Docker; docker run --rm -v "${PWD}:/workspace" -w /workspace local/sbom-syft:latest dir:. -o spdx-json=sbom.spdx.json }
  "secrets"    { Assert-Docker; docker run --rm -v "${PWD}:/workspace" -w /workspace local/secrets-gitleaks:latest detect --no-banner --redact }
  "dockerlint" {
  Assert-Docker
  # Only lint our tool Dockerfiles; ignore backups and the legacy plain "Dockerfile"
  $files = Get-ChildItem tools/docker -File |
           Where-Object { $_.Name -like 'Dockerfile.*' -and $_.Name -notmatch '\.bak(\d+)?$' } |
           ForEach-Object { "tools/docker/$($_.Name)" }

  if (-not $files) { throw "No Dockerfiles found under tools/docker" }

  # NOTE: $files must be on the same line so it’s passed as args
  docker run --rm -v "${PWD}:/workspace" -w /workspace local/dockerlint:latest $files
}
}
