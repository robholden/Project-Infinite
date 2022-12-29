# Stage 1

# We label our stage as ‘node’
FROM node:latest as node

ARG FONTAWESOME_NPM_AUTH_TOKEN

WORKDIR /src/Shared
COPY ./Shared .
RUN npm i

WORKDIR /src/Desktop
COPY ./Desktop .
RUN npm i

# Build the angular app in production mode and store the artifacts in dist folder
RUN npm run build

# Stage 2
FROM nginx:alpine
COPY Desktop/nginx-custom.conf /etc/nginx/nginx.conf
COPY --from=node /src/Desktop/dist /usr/share/nginx/html

# docker build -t iamrobert/project-infinite-ui-ng:latest --build-arg FONTAWESOME_NPM_AUTH_TOKEN=${FONTAWESOME_NPM_AUTH_TOKEN} -f Desktop/Dockerfile .
