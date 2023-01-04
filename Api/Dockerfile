FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder
COPY ./ /kafe/src
RUN dotnet publish /kafe/src --configuration Release --output /kafe/publish --runtime linux-x64 --self-contained

FROM mcr.microsoft.com/dotnet/aspnet:7.0
COPY --from=builder /kafe/publish /kafe
WORKDIR /kafe
