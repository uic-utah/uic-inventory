set -e
cd "$(dirname "$0")/.."

echo 'building image'

docker-compose build --pull api
