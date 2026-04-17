require('dotenv').config(); // Cargar variables de entorno lo primero de todo
const express = require('express');
const cors = require('cors');
const http = require('http');

// 1. Importar Base de Datos y WebSockets
const { connectMongo } = require('./src/Infrastructure/database/db');
const SocketServer = require('./src/Infrastructure/websockets/SocketServer');

// 2. Importar Repositorios (Capa de Datos - MongoDB Nativo)
const UserMongoRepository = require('./src/Infrastructure/repositories/UserMongoRepository');
const GameMongoRepository = require('./src/Infrastructure/repositories/GameMongoRepository');
const ScoreMongoRepository = require('./src/Infrastructure/repositories/ScoreMongoRepository');

// 3. Importar Servicios (Capa Lógica de Negocio)
const UserService = require('./src/Application/UserService');
const GameService = require('./src/Application/GameService');
const ScoreService = require('./src/Application/ScoreService');

// 4. Importar Controladores (Capa de Presentación HTTP)
const UserController = require('./src/Infrastructure/http/UserController');
const GameController = require('./src/Infrastructure/http/GameController');
const ScoreController = require('./src/Infrastructure/http/ScoreController');

// 5. Importar Rutas
const userRoutes = require('./src/Infrastructure/http/userRoutes');
const gameRoutes = require('./src/Infrastructure/http/gameRoutes'); // Descomentar cuando crees el archivo
const scoreRoutes = require('./src/Infrastructure/http/scoreRoutes'); // Descomentar cuando crees el archivo

const app = express();
const PORT = process.env.PORT || 3000;

// Middlewares básicos
app.use(cors());
app.use(express.json());

// Crear el servidor HTTP explícito (necesario para acoplar WebSockets)
const server = http.createServer(app);

// Inicializar el servidor de WebSockets
const socketServer = new SocketServer(server);

// Función principal para arrancar todo en orden usando async/await
const startServer = async () => {
    try {
        // A. Conectar a MongoDB nativo ANTES de inicializar nada más
        await connectMongo();

        // B. Inyección de Dependencias (Instanciar las clases como piezas de Lego)
        const userRepository = new UserMongoRepository();
        const gameRepository = new GameMongoRepository();
        const scoreRepository = new ScoreMongoRepository();

        const userService = new UserService(userRepository);
        const gameService = new GameService(gameRepository);
        // ScoreService necesita GameService para poder cerrar la partida cuando se guarda la puntuación
        const scoreService = new ScoreService(scoreRepository, gameService);

        const userController = new UserController(userService);
        const gameController = new GameController(gameService);
        const scoreController = new ScoreController(scoreService, gameService);

        // C. Montar las rutas en Express
        app.use('/api/users', userRoutes(userController));
        app.use('/api/games', gameRoutes(gameController));
        app.use('/api/scores', scoreRoutes(scoreController));

        // Ruta de prueba (Health Check) para comprobar que el server vive
        app.get('/api/health', (req, res) => {
            res.status(200).json({ status: 'Neon Survivor Server is RUNNING y conectado a MongoDB' });
        });

        // D. Levantar el servidor HTTP y WebSocket a la vez
        server.listen(PORT, () => {
            console.log(`Servidor HTTP (REST) escuchando en http://localhost:${PORT}`);
            console.log(`Servidor de Tiempo Real (WebSocket) escuchando en ws://localhost:${PORT}`);
        });

    } catch (error) {
        console.error("Error fatal arrancando el servidor:", error);
        process.exit(1);
    }
};

startServer();