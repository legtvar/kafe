FROM archlinux:latest
RUN sed -i '/NoExtract.*i18n/d' /etc/pacman.conf \
    && sed -i '/NoExtract.*locale/d' /etc/pacman.conf \
    && sed -i '/NoExtract.*man/d' /etc/pacman.conf \
    && sed -i '/NoExtract.*help/d' /etc/pacman.conf
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
    npm
RUN git lfs install
RUN mandb
RUN sed -i '/en_US/s/^#//g' /etc/locale.gen \
    && sed -i '/cs_CZ/s/^#//g' /etc/locale.gen \
    && sed -i '/sk_SK/s/^#//g' /etc/locale.gen \
    && locale-gen
RUN npm install -g pnpm
COPY ["post-create.sh", "post-attach.sh", "/root"]
