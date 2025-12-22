FROM archlinux:latest
RUN sed -i '/NoExtract.*usr\/share\/i18n/d' /etc/pacman.conf \
    && sed -i '/NoExtract.*usr\/share\/man/d' /etc/pacman.conf \
    && sed -i '/NoExtract.*usr\/share\/help/d' /etc/pacman.conf
RUN pacman -Syu --noconfirm && pacman -S --noconfirm \
    openssh \
    git \
    git-lfs \
    man-db \
    man-pages \
    dotnet-sdk \
    dotnet-sdk-9.0 \
    aspnet-runtime \
    aspnet-runtime-9.0 \
    ffmpeg \
    postgresql \
    nodejs \
    npm \
    glibc \
    glibc-locales
RUN git lfs install
RUN mandb
RUN npm install -g pnpm
COPY ["post-create.sh", "post-attach.sh", "/root"]
