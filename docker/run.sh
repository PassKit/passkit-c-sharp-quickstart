#!/usr/bin/env bash
set -e

if [ -z "$KEY_PASSWORD" ]; then
  echo "ERROR: KEY_PASSWORD environment variable not set."
  exit 1
fi

echo "Cloning PassKit C# Quickstart..."
git clone https://github.com/PassKit/passkit-c-sharp-quickstart.git
cd passkit-c-sharp-quickstart

echo "Copying credentials into certs/..."
mkdir -p certs
cp /app/credentials/* certs/

echo "🔐 Decrypting key.pem with provided KEY_PASSWORD..."
openssl ec -in certs/key.pem -passin pass:"$KEY_PASSWORD" -out certs/key-decrypted.pem

echo "🔁 Creating client.pfx from certificate.pem, key-decrypted.pem, and ca-chain.pem..."
openssl pkcs12 -export \
  -in certs/certificate.pem \
  -inkey certs/key-decrypted.pem \
  -certfile certs/ca-chain.pem \
  -out certs/client.pfx \
  -passout pass:

echo "🔍 Final contents of certs directory:"
ls -lh certs/

echo "Restoring and building .NET 6.0 solution..."
dotnet restore
dotnet build --configuration Release

echo "Running all Quickstart flows…"
dotnet run --configuration Release

echo "Completed all C# examples."
