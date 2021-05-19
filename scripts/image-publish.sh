set -e
cd "$(dirname "$0")/.."

./image-build.sh
./image-tag.sh
./image-publish.sh
