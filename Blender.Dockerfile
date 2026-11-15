FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

COPY ./ /kafe/src

RUN apk add --no-cache git

RUN git config --global --add safe.directory /kafe/src

ARG KAFE_VERSION=""

RUN if [ -n "$KAFE_VERSION" ]; then extra_args="-p:Version=$KAFE_VERSION -p:GitVersion=false"; fi; \
    dotnet publish /kafe/src/Pigeons/Kafe.Pigeons.csproj \
        --configuration Release \
        --output /kafe/publish \
        --runtime linux-musl-x64 \
        --self-contained false \
        ${extra_args}

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
RUN apk add --no-cache blender-headless

COPY --from=builder /kafe/publish /app
COPY ./Pigeons/pigeons_setup.py /app/pigeons_setup.py

ENV PATH="/usr/bin/blender:/usr/bin/blender-headless:/root/.config/blender/4.4/extensions/repository/pigeons:${PATH}"

RUN blender-headless --background --python "/app/pigeons_setup.py" -- \
    --repo-name="muni.cz" \
    --repo-url="https://muni.cz/go/pigeons-repo" \
    --id="pigeons" \
    --allow-online

EXPOSE 8042
WORKDIR /app
CMD ["/app/Kafe.Pigeons"]
