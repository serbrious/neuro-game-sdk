# use the latest node base container
FROM node:latest

WORKDIR /app

COPY package.json package.json

RUN npm install

COPY index.ts tsconfig.json ./

# Webserver port
EXPOSE 1337
# Websocket URL
EXPOSE 8000

CMD ["npm", "start"]