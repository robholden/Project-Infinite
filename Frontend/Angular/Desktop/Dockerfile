# Stage 1

# We label our stage as ‘builder’
FROM node:17.3-alpine as builder

# Build Shared
WORKDIR /src
COPY Shared/package.json Shared/package-lock.json ./Shared/

WORKDIR /src/Shared
RUN npm i --legacy-peer-deps

# Build desktop
WORKDIR /src
ARG FONTAWESOME_NPM_AUTH_TOKEN
COPY Desktop/.npmrc Desktop/package.json Desktop/package-lock.json ./Desktop/

WORKDIR /src/Desktop
RUN npm i --legacy-peer-deps

WORKDIR /src
COPY . .

# Build the angular app in production mode and store the artifacts in dist folder
WORKDIR /src/Desktop
RUN $(npm bin)/ng build --configuration=production --output-path=dist

# Stage 2
FROM nginx:1.17.1-alpine
COPY Desktop/nginx-custom.conf /etc/nginx/nginx.conf
COPY --from=builder /src/Desktop/dist /usr/share/nginx/html

# docker build -t iamrobert/project-infinite-ui-ng:latest --build-arg FONTAWESOME_NPM_AUTH_TOKEN=${FONTAWESOME_NPM_AUTH_TOKEN} -f Desktop/Dockerfile .
