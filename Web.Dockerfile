FROM node:18 as builder
WORKDIR /app
COPY ./Web/package*.json ./
RUN npm ci
COPY ./Web ./
RUN npm run build

FROM nginx:alpine
RUN mkdir /app
COPY --from=builder /app/build /app
COPY --from=builder /app/nginx.conf /etc/nginx/nginx.conf
