# ============================================================================
# Imagem do frontend: build com Vite, servido por Nginx.
# Contexto de build: ./frontend
# ============================================================================

# --- Estagio 1: build do bundle ---------------------------------------------
FROM node:26-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# A URL da API e resolvida em tempo de BUILD pelo Vite (import.meta.env),
# entao precisa vir como argumento e nao como variavel de runtime.
ARG VITE_API_URL=http://localhost:5080
ARG VITE_APP_NOME="Nimbus ERP"
ENV VITE_API_URL=$VITE_API_URL \
    VITE_APP_NOME=$VITE_APP_NOME

RUN npm run build

# --- Estagio 2: servidor estatico -------------------------------------------
FROM nginx:1.29-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://127.0.0.1:8080/ || exit 1
