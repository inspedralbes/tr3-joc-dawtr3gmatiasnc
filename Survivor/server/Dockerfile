# Usa una imagen oficial de Node.js ligera
FROM node:20-alpine

# Establece el directorio de trabajo dentro del contenedor
WORKDIR /usr/src/app

# Copia primero los archivos de dependencias para aprovechar el caché de Docker
COPY package*.json ./

# Instala las dependencias (usa npm ci si tienes package-lock.json para instalaciones más exactas)
RUN npm install

# Copia el resto del código de la aplicación (incluyendo src/, config/, server.js, etc.)
COPY . .

# Expone el puerto que usa tu aplicación (ajusta el 3000 si tu app usa otro)
EXPOSE 3000

# Comando para iniciar la aplicación
CMD ["node", "server.js"]