# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package.json ./

# Remove package-lock.json if it exists and install fresh for Alpine platform
# This ensures platform-specific optional dependencies are correctly installed
RUN npm install

# Copy source code
COPY . .

# ARG for build-time environment variables (placed AFTER source copy for optimal caching)
ARG VITE_API_URL=http://localhost:5002/api
ARG VITE_APP_NAME=RewardStar
ARG VITE_GOOGLE_CLIENT_ID=

# Set environment variables for Vite build
ENV VITE_API_URL=${VITE_API_URL}
ENV VITE_APP_NAME=${VITE_APP_NAME}
ENV VITE_GOOGLE_CLIENT_ID=${VITE_GOOGLE_CLIENT_ID}

# Build application
RUN npm run build

# Runtime stage - serve with nginx
FROM nginx:alpine

# ARG for runtime configuration (needed for nginx.conf template)
ARG API_PORT=5002

# Copy built application from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration template
COPY nginx.conf /etc/nginx/templates/default.conf.template

# Set environment variable for nginx template substitution
ENV API_PORT=${API_PORT}

# Expose port
EXPOSE 80

# Start nginx (will process templates automatically)
CMD ["nginx", "-g", "daemon off;"]
