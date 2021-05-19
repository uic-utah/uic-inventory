set -e

cd "$(dirname "$0")/.."

dotnet publish ./src/api/api.csproj -c Release -r linux-x64
