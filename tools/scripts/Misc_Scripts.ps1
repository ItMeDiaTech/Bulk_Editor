#Infra?
docker compose -f infra\docker-compose.yml up -d
docker compose -f infra\docker-compose.yml down

#For Portainer:
# persistent data
docker volume create portainer_data

# run Portainer CE (Linux containers)
docker run -d --name portainer --restart=always `
  -p 9000:9000 -p 9443:9443 `
  -v /var/run/docker.sock:/var/run/docker.sock `
  -v portainer_data:/data `
  portainer/portainer-ce:latest


#
#Portainer: https://localhost:9443 (or http://localhost:9000)
#
#Dozzle: http://localhost:9999
#
#Open WebUI: http://localhost:3000
#
#Scout:
#docker scout quickview
#docker scout cves local/dotnet-diag:latest
#docker scout cves --only-fixed local/dotnet-lint:latest
#docker scout compare local/dotnet-lint:latest mcr.microsoft.com/dotnet/sdk:8.0
#
#
#docker compose -f infra\docker-compose.yml pull
#docker system df            # disk use by images/containers/volumes
#docker image ls             # what you have locally

