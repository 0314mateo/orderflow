# Etapa 1: build — compila el bundle estático de producción con Vite
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

# Vite "hornea" las variables VITE_* dentro del bundle en tiempo de BUILD,
# no en tiempo de ejecución — por eso se recibe como build-arg, no como env var normal.
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

# Etapa 2: runtime — nginx sirviendo solo los archivos estáticos ya compilados
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80