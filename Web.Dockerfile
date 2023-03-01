FROM node:18 as builder
WORKDIR /web
COPY ./Web/package*.json ./
RUN npm ci
COPY ./Web ./
RUN npm run build

FROM nginx:alpine
RUN mkdir /web
COPY --from=builder /web/build /web
COPY --from=builder /web/nginx.conf /etc/nginx.conf
