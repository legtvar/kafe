FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
COPY ./ /kafe/src
RUN apk update && apk add git
RUN dotnet publish /kafe/src/Pigeons/Pigeons.csproj --configuration Release --output /kafe/publish --runtime linux-musl-x64 --self-contained

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine
RUN apk update && apk add blender-headless
COPY --from=builder /kafe/publish /app
COPY ./Pigeons/pigeons_setup.py /app/pigeons_setup.py
ENV PATH="/usr/bin/blender:/usr/bin/blender-headless:/root/.config/blender/4.4/extensions/repository/pigeons:${PATH}"
RUN blender-headless --background --python "/app/pigeons_setup.py" -- --repo-name="muni.cz" --repo-url="https://muni.cz/go/pigeons-repo" --id="pigeons" --allow-online

EXPOSE 8042

CMD ["/app/Pigeons"]