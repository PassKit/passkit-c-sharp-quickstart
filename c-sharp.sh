#!/bin/bash
set -e

CLEAN=false

for arg in "$@"; do
  if [ "$arg" == "--clean" ]; then
    CLEAN=true
  fi
done

# Ensure credentials and constants exist before continuing
if [ ! -f ./certs/certificate.pem ] || [ ! -f ./certs/key.pem ] || [ ! -f ./certs/ca-chain.pem ]; then
  echo "❌ Required credential files (certificate.pem, key.pem, ca-chain.pem) are missing."
  exit 1
fi

# Copy credentials and constants into Docker build context
mkdir -p ./docker/credentials
cp ./certs/* ./docker/credentials/
cp ./Quickstarts/Constants.cs ./docker/Constants.cs

docker build -t csharp-quickstart-runner ./docker

# Run container with KEY_PASSWORD for private key decryption (if applicable)
docker run --rm -e KEY_PASSWORD=password csharp-quickstart-runner

if [ "$CLEAN" = true ]; then
  echo "Cleaning up Docker image..."
  docker rmi csharp-quickstart-runner
fi

# Cleanup copied build context files
rm -rf ./docker/credentials ./docker/Constants.cs
