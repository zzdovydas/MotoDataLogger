# Use an official Node.js runtime as the base image
FROM node:18-alpine

# Set the working directory inside the container
WORKDIR /app

# Copy package.json and package-lock.json to the working directory
COPY MotoDataLoggerFrontend/package*.json ./

# Install any needed dependencies
RUN npm install

# Copy the rest of the application code to the working directory
COPY MotoDataLoggerFrontend/ .

# Build the Vue.js application for production
RUN npm run build

# Use a smaller base image for serving static files
FROM nginx:alpine

# Copy the built Vue.js application from the previous stage
COPY --from=0 /app/dist /usr/share/nginx/html

# Copy nginx config file
COPY MotoDataLoggerFrontend/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
