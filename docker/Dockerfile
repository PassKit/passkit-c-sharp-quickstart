FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim

RUN sed -i \
      -e 's|^deb |deb [trusted=yes] |' \
      -e 's|^deb-src |deb-src [trusted=yes] |' \
      -e 's|http://|https://|g' \
      /etc/apt/sources.list \
 && apt-get update \
 && apt-get install -y --no-install-recommends --no-upgrade git \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY credentials/ ./credentials/
COPY Constants.cs /app/Constants.cs

COPY run.sh .
RUN chmod +x run.sh

ENTRYPOINT ["./run.sh"]
