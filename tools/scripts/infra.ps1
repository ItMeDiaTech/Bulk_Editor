param(
  [ValidateSet("up","down","logs","pull")]$cmd="up",
  [string]$model="qwen2.5-coder:7b"
)
$compose = "infra\docker-compose.yml"
switch ($cmd) {
  "up"   { docker compose -f $compose up -d }
  "down" { docker compose -f $compose down }
  "logs" { docker compose -f $compose logs -f }
  "pull" { docker exec -it ollama ollama pull $model }
}