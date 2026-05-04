# =============================================================================
# Stage 1: Build
# =============================================================================
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --ignore-scripts

COPY . .

# VITE_API_URL é resolvido em build-time pelo Vite
# Em docker-compose, passe --build-arg VITE_API_URL=/api (nginx faz proxy)
ARG VITE_API_URL=/api
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

# =============================================================================
# Stage 2: Serve com nginx
# =============================================================================
FROM nginx:1.27-alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
