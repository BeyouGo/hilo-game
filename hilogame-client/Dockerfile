# ---------- Build Angular ----------
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
# Adjust for your Angular project name if needed
RUN npm run build -- --configuration production

# ---------- Serve with Nginx ----------
FROM nginx:alpine AS runtime
# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Copy Angular dist output (adjust path if your output folder differs)
COPY --from=build /app/dist/mehdichat-cli/browser /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
