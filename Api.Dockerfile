FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
COPY ./ /kafe/src
RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && rm -rf /var/lib/apt/lists/*

RUN dotnet restore --locked-mode /kafe/src/Api/Kafe.Api.csproj && \
    dotnet publish --no-restore /kafe/src/Api/Kafe.Api.csproj \
                    --configuration Release \
                    --output /kafe/publish \
                    --runtime linux-x64 \
                    --self-contained false \
                    -p:GitVersion=false

RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg

# Generate the certificate while the SDK is still available.
RUN dotnet dev-certs https --trust

FROM mcr.microsoft.com/dotnet/aspnet:9.0
ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*

# Required by the HTTPS endpoint in docker-compose.local.yml.
COPY --from=builder /root/.aspnet /root/.aspnet
COPY --from=builder /root/.dotnet/corefx/cryptography/x509stores \
                    /root/.dotnet/corefx/cryptography/x509stores

COPY --from=builder /root/vsdbg /root/vsdbg

COPY --from=builder /kafe/publish /app
WORKDIR /app
CMD ["/app/Kafe.Api"]
