FROM denoland/deno:latest AS builder
COPY . /src
RUN cd /src && deno task build

FROM nginx
ARG PREFIX="/docs"
ENV PREFIX=$PREFIX
COPY --from=builder /src/public /usr/share/nginx/html${PREFIX}
