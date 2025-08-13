param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('run','apply','apply-clipboard')]
  [string]$cmd,
  [string]$arg
)

$ErrorActionPreference = 'Stop'

# Paths
$root       = (Resolve-Path .).Path
$img        = 'local/agent-dotnet:latest'
$dockerfile = Join-Path $root 'tools\docker\Dockerfile'
$agentDll   = Join-Path $root 'tools\agent\bin\Debug\net8.0\AgentRunner.dll'

# Build agent if missing
if (-not (Test-Path $agentDll)) {
  dotnet build 'tools\agent\AgentRunner.csproj'
}

# Ensure image exists/updated
docker build -f $dockerfile -t $img $root | Out-Null

switch ($cmd) {

  'run' {
    # $arg can be build|test|lint|format
    $runArgs = @(
      '--rm',
      '-v', "${root}:/workspace",
      '-w', '/workspace',
      $img,
      'dotnet','exec','/workspace/tools/agent/bin/Debug/net8.0/AgentRunner.dll','run', $arg
    )
    docker run @runArgs
  }

  'apply' {
    if (-not $arg) { throw 'Provide path to a unified diff file (.patch).' }
    $patchPath = (Resolve-Path $arg).Path
    $runArgs = @(
      '--rm',
      '-v', "${root}:/workspace",
      '-v', "${patchPath}:/tmp/diff.patch:ro",
      '-w', '/workspace',
      $img,
      'dotnet','exec','/workspace/tools/agent/bin/Debug/net8.0/AgentRunner.dll','apply','/tmp/diff.patch'
    )
    docker run @runArgs
  }

  'apply-clipboard' {
    $tmp = New-TemporaryFile
    (Get-Clipboard) | Set-Content -NoNewline -Path $tmp
    try {
      $runArgs = @(
        '--rm',
        '-v', "${root}:/workspace",
        '-v', "${tmp}:/tmp/diff.patch:ro",
        '-w', '/workspace',
        $img,
        'dotnet','exec','/workspace/tools/agent/bin/Debug/net8.0/AgentRunner.dll','apply','/tmp/diff.patch'
      )
      docker run @runArgs
    }
    finally {
      Remove-Item $tmp -ErrorAction SilentlyContinue
    }
  }
}
