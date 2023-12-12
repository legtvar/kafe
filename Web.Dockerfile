FROM node:20 as builder
RUN corepack enable && corepack prepare pnpm@latest --activate
WORKDIR /app
COPY ./Web/package*.json ./
RUN pnpm i
COPY ./Web ./
RUN pnpm run build

FROM nginx:alpine
RUN mkdir /app
COPY --from=builder /app/build /app
COPY --from=builder /app/nginx.conf /etc/nginx/nginx.conf
