FROM archlinux:latest
RUN pacman -Syu --noconfirm
RUN pacman -S --noconfirm dotnet-sdk dotnet-sdk-9.0 ffmpeg postgresql nodejs npm git
RUN npm install -g pnpm
