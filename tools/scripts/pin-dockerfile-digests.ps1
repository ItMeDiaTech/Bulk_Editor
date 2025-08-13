# Pins all FROM lines in tools/docker/Dockerfile* to digests.
param([string]$Dir = "tools/docker")
$ErrorActionPreference = "Stop"

$files = Get-ChildItem -Path $Dir -File -Filter 'Dockerfile*'
if (-not $files) { throw "No Dockerfiles in $Dir" }

# Matches: FROM <image>[:tag] [AS stage]
$FROM_RE = '^(FROM\s+)([^\s@:]+(?:/[^\s@:]+)*)(?::([^\s@]+))?(\s+AS\s+\w+)?\s*$'

foreach ($f in $files) {
  $orig = Get-Content $f.FullName -Raw

  $new = [regex]::Replace($orig, $FROM_RE, {
    param($m)
    $prefix = $m.Groups[1].Value
    $image  = $m.Groups[2].Value
    $tag    = if ($m.Groups[3].Success) { $m.Groups[3].Value } else { "latest" }
    $stage  = if ($m.Groups[4].Success) { $m.Groups[4].Value } else { "" }

    Write-Host "Pulling ${image}:${tag}..."
    docker pull "${image}:${tag}" | Out-Null

    $digest = (docker inspect --format "{{index .RepoDigests 0}}" "${image}:${tag}").Trim()
    if (-not $digest) { throw "No digest for ${image}:${tag}" }

    return "$prefix$digest$stage"
  }, 'Multiline')

  if ($new -ne $orig) {
    Copy-Item $f.FullName "$($f.FullName).bak" -Force
    [System.IO.File]::WriteAllText($f.FullName, $new, [System.Text.UTF8Encoding]::new($false))
    Write-Host "[pinned] $($f.Name)"
  } else {
    Write-Host "[skip]   $($f.Name) (no change)"
  }
}
