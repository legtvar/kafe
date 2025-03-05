FROM alpine:latest AS builder
RUN apk update && apk add nodejs-current npm
RUN corepack enable && npm install -g pnpm
WORKDIR /app
COPY ./Web ./
RUN pnpm i
RUN pnpm run build

FROM nginx:alpine
RUN mkdir /app
COPY --from=builder /app/build /app
COPY --from=builder /app/nginx.conf /etc/nginx/nginx.conf
