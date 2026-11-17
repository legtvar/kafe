FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder

COPY ./ /kafe/src

RUN apk add --no-cache git openssl

RUN git config --global --add safe.directory /kafe/src

ARG KAFE_VERSION=""

RUN if [ -n "$KAFE_VERSION" ]; then extra_args="-p:Version=$KAFE_VERSION -p:GitVersion=false"; fi; \
    dotnet restore --locked-mode /kafe/src/Api/Kafe.Api.csproj && \
    dotnet publish --no-restore /kafe/src/Api/Kafe.Api.csproj \
                    --configuration Release \
                    --output /kafe/publish \
                    --runtime linux-musl-x64 \
                    --self-contained false \
                    ${extra_args}

RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

# Generate the certificate while the SDK is still available.
# .NET 10 requires the trust directory in SSL_CERT_DIR on Linux.
RUN SSL_CERT_DIR="$HOME/.aspnet/dev-certs/trust:/etc/ssl/certs" \
    dotnet dev-certs https --trust

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

# Alpine's ASP.NET image defaults to invariant globalization, but KAFE needs
# real culture data for its supported languages.
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache ffmpeg icu-data-full icu-libs krb5-libs

# Required by the HTTPS endpoint in docker-compose.local.yml.
COPY --from=builder /root/.aspnet /root/.aspnet
COPY --from=builder /root/.dotnet/corefx/cryptography/x509stores \
                    /root/.dotnet/corefx/cryptography/x509stores

COPY --from=builder /vsdbg /root/vsdbg
COPY --from=builder /kafe/publish /app
WORKDIR /app
CMD ["/app/Kafe.Api"]
