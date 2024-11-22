FROM mcr.microsoft.com/dotnet/sdk:9.0 as builder
COPY ./ /kafe/src
RUN apt install -y git
RUN dotnet publish /kafe/src/Api/Kafe.Api.csproj --configuration Release --output /kafe/publish --runtime linux-x64 --self-contained

FROM mcr.microsoft.com/dotnet/sdk:9.0
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update && apt install -y ffmpeg && rm -rf /var/lib/apt/lists/*
RUN dotnet dev-certs https --trust
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg
COPY --from=builder /kafe/publish /app
WORKDIR /app
CMD /app/Kafe.Api
