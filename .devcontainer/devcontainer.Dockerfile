ARG DOTNET_VERSION=9.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}

ARG NVM_VERSION=v0.40.4
ARG NODE_VERSION=24
ENV DEBIAN_FRONTEND=noninteractive

# unignore manpages
RUN sed -i 's:^path-exclude=/usr/share/man:#path-exclude=/usr/share/man:' /etc/dpkg/dpkg.cfg.d/docker

RUN apt-get update && apt-get install -y \
    curl \
    openssh-client \    
    git \
    git-lfs \
    locales \
    ffmpeg \
    manpages \
    manpages-dev \
    man-db

# ensure EN, CZ, SK locales exist
RUN sed -i '/en_US.UTF-8/s/^# //g' /etc/locale.gen \
    && sed -i '/cs_CZ.UTF-8/s/^# //g' /etc/locale.gen \
    && sed -i '/sk_SK.UTF-8/s/^# //g' /etc/locale.gen \
    && locale-gen

RUN mandb

RUN git lfs install

RUN dotnet dev-certs https --trust

RUN useradd --create-home --shell /bin/bash barista

COPY ["post-start.sh", "post-attach.sh", "/home/barista"]

USER barista

RUN curl -o- "https://raw.githubusercontent.com/nvm-sh/nvm/$NVM_VERSION/install.sh" | bash

ENV NVM_DIR=/home/barista/.nvm

RUN bash -c "source $NVM_DIR/nvm.sh && nvm install $NODE_VERSION"

RUN bash -c "source $NVM_DIR/nvm.sh && npm install -g pnpm"

