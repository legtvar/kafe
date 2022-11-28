FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
COPY ./ /kafe/src
RUN dotnet publish /kafe/src --configuration Release --output /kafe/publish --runtime linux-x64

FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY --from=builder /kafe/publish /kafe
