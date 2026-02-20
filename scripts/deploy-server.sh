#!/bin/bash
# Ejecutar en el servidor desde la raíz del repo (donde está .git).
# Uso: ./NotasApi/scripts/deploy-server.sh
# O desde /cloudclusters/NotasApi: ./scripts/deploy-server.sh

set -e

# Si el script está en NotasApi/scripts/, el proyecto está un nivel arriba
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [ -d "$SCRIPT_DIR/../.git" ]; then
  REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
else
  # Repo puede estar en el padre (Anota/NotasApi)
  REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
fi

cd "$REPO_ROOT"
echo "==> Repo: $REPO_ROOT"

echo "==> git pull"
git pull

# Buscar la carpeta NotasApi (puede ser ./NotasApi o . si estamos dentro de NotasApi)
if [ -f "NotasApi/NotasApi.csproj" ]; then
  API_DIR="$REPO_ROOT/NotasApi"
elif [ -f "NotasApi.csproj" ]; then
  API_DIR="$REPO_ROOT"
else
  echo "No se encontró NotasApi.csproj en $REPO_ROOT ni en $REPO_ROOT/NotasApi"
  exit 1
fi

cd "$API_DIR"
echo "==> Publicando desde $API_DIR"
rm -rf ./publish
dotnet publish -c Release -o ./publish

echo "==> Listo. Reinicia el backend manualmente según tu entorno:"
echo "    - systemd: sudo systemctl restart notasapi"
echo "    - PM2:     pm2 restart notasapi"
echo "    - Docker:  docker compose restart api"
echo "    - Manual:  pkill -f NotasApi; cd $API_DIR/publish && nohup dotnet NotasApi.dll &"
