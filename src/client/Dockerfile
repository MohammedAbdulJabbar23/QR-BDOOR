FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS public
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.public.conf /etc/nginx/conf.d/default.conf

FROM nginx:1.27-alpine AS internal
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.internal.conf /etc/nginx/conf.d/default.conf
