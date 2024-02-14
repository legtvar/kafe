FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder
COPY ./ /kafe/src
RUN apt install -y git
RUN dotnet publish /kafe/src/Announcer --configuration Release --output /publish --framework net7.0 --runtime linux-x64 --self-contained

FROM mcr.microsoft.com/dotnet/sdk:7.0
ENV DEBIAN_FRONTEND=noninteractive
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg
COPY --from=builder /publish /app
WORKDIR /app
ENTRYPOINT /app/Kafe.Announcer
