FROM alpine:latest as builder
RUN apk update && apk add nodejs-current npm
RUN corepack enable && corepack prepare pnpm@latest --activate
WORKDIR /app
COPY ./Web ./
RUN pnpm i
RUN pnpm run build

FROM nginx:alpine
RUN mkdir /app
COPY --from=builder /app/build /app
COPY --from=builder /app/nginx.conf /etc/nginx/nginx.conf
