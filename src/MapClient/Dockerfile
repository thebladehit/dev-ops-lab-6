FROM node:18-alpine AS builder
WORKDIR /app
COPY . .
RUN npm ci
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
RUN rm -f /usr/share/nginx/html/config.json
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]