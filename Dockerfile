FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder
COPY ./ /kafe/src
RUN dotnet publish /kafe/src/Api --configuration Release --output /kafe/publish --framework net7.0 --runtime linux-x64 --self-contained

FROM mcr.microsoft.com/dotnet/aspnet:7.0
COPY --from=builder /kafe/publish /kafe
WORKDIR /kafe
