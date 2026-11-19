FROM node:24-alpine AS build
WORKDIR /app
RUN npm install -g pnpm

COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist/*/browser /usr/share/nginx/html
EXPOSE 80
