const WebSocket = require('ws');
const Room = require('../../Domain/Room');
const RoomMongoRepository = require('../repositories/RoomMongoRepository');

class SocketServer {
    constructor(server) {
        this.wss = new WebSocket.Server({ server });

        // NUEVO: Añadimos dirX y dirY al comentario para recordar qué guardamos
        this.clients = new Map(); // ws -> { playerId, roomId, x, y, dirX, dirY }
        this.rooms = new Map();   // roomId -> Room instance
        this.roomRepo = new RoomMongoRepository();
        this.scoreService = null; // S'injectarà des de server.js
        this.gameService = null;  // S'injectarà des de server.js

        this.wss.on('connection', (ws) => {
            console.log('Nou jugador connectat al WebSocket!');

            ws.on('message', async (message) => {
                await this.handleMessage(ws, message);
            });

            ws.on('close', async () => {
                console.log('Jugador desconnectat del WebSocket');
                await this.handleDisconnect(ws);
            });
        });
    }

    setScoreService(scoreService) {
        this.scoreService = scoreService;
    }

    setGameService(gameService) {
        this.gameService = gameService;
    }

    async handleMessage(ws, message) {
        try {
            const data = JSON.parse(message);
            const playerInfo = this.clients.get(ws) || {};

            switch (data.type) {
                case 'CREATE_ROOM': {
                    console.log('Mensaje recibido: CREATE_ROOM');
                    // Generamos un código corto de 4 dígitos si no se proporciona uno
                    const newRoomId = data.roomId || Math.floor(1000 + Math.random() * 9000).toString();
                    const roomName = data.roomName || `Sala de ${data.playerId || 'jugador'}`;

                    const newRoom = new Room(newRoomId, roomName);
                    this.rooms.set(newRoomId, newRoom);

                    // Guardar en MongoDB
                    await this.roomRepo.create({
                        id: newRoom.id,
                        name: newRoom.name,
                        maxPlayers: newRoom.maxPlayers,
                        status: newRoom.status
                    });

                    // Guardar a la col·lecció GAMES explícitament
                    if (this.gameService) {
                        try {
                            await this.gameService.startGame(newRoom.id);
                        } catch (e) {
                            console.error("Error guardant a GAMES: ", e);
                        }
                    }

                    // NUEVO: Asociamos al creador con la sala de inmediato en el servidor
                    if (data.playerId) {
                        this.clients.set(ws, {
                            playerId: data.playerId,
                            roomId: newRoomId,
                            x: 0, y: 0, dirX: 1, dirY: 0
                        });
                    }

                    ws.send(JSON.stringify({ type: 'ROOM_CREATED', roomId: newRoomId }));
                    console.log(`Sala creada: ${newRoomId}`);
                    break;
                }
                case 'JOIN_ROOM': {
                    console.log('Mensaje recibido: JOIN_ROOM');
                    const { roomId, playerId } = data;
                    let roomToJoin = this.rooms.get(roomId);

                    if (!roomToJoin) {
                        const dbRoom = await this.roomRepo.findById(roomId);
                        if (dbRoom) {
                            roomToJoin = new Room(dbRoom.id, dbRoom.name, dbRoom.maxPlayers);
                            if (dbRoom.players) {
                                dbRoom.players.forEach(pId => roomToJoin.addPlayer(pId));
                            }
                            this.rooms.set(roomId, roomToJoin);
                        } else {
                            ws.send(JSON.stringify({ type: 'ERROR', message: 'La sala no existeix' }));
                            return;
                        }
                    }

                    if (roomToJoin.addPlayer(playerId)) {
                        // NUEVO: Inicializamos dirX y dirY también
                        this.clients.set(ws, { playerId, roomId, x: 0, y: 0, dirX: 1, dirY: 0 });
                        await this.roomRepo.addPlayerToDb(roomId, playerId);

                        console.log(`Jugador ${playerId} s'ha unit a la sala ${roomId}.`);
                        this.broadcastToRoom(roomId, ws, { type: 'PLAYER_JOINED', playerId });
                        ws.send(JSON.stringify({ type: 'JOINED_ROOM', roomId }));
                    } else {
                        // Opcional: Si el jugador ya estaba en la sala (p.ej. por reconexión), actualizamos su socket info
                        const existingInfo = this.clients.get(ws);
                        if (existingInfo && existingInfo.playerId === playerId && existingInfo.roomId === roomId) {
                            ws.send(JSON.stringify({ type: 'JOINED_ROOM', roomId }));
                        } else {
                            ws.send(JSON.stringify({ type: 'ERROR', message: 'La sala està plena o ja hi ets' }));
                        }
                    }
                    break;
                }
                case 'MOVE': {
                    // console.log('Mensaje recibido: MOVE'); // Demasiado ruido si se loguea siempre
                    if (playerInfo.roomId) {
                        playerInfo.x = data.x;
                        playerInfo.y = data.y;
                        // NUEVO: Guardamos y reenviamos la dirección hacia donde mira el jugador
                        playerInfo.dirX = data.dirX;
                        playerInfo.dirY = data.dirY;

                        this.clients.set(ws, playerInfo);
                        this.broadcastToRoom(playerInfo.roomId, ws, data);
                    } else {
                        this.broadcast(ws, data);
                    }
                    break;
                }
                case 'ATTACK': {
                    console.log('Mensaje recibido: ATTACK');
                    if (playerInfo.roomId) {
                        const room = this.rooms.get(playerInfo.roomId);

                        // --- NUEVO: RASTREO DE PUNTOS POR MUERTES ---
                        if (data.message === 'DEATH' && room) {
                            // Si alguien muere, buscamos al OTRO jugador en la sala para darle un punto
                            const winnerId = room.players.find(id => id !== playerInfo.playerId);
                            if (winnerId) {
                                room.updateScore(winnerId, 1);
                                console.log(`Punto para ${winnerId}. Score actual:`, room.scores);

                                // Broadcast de la puntuación actualizada
                                this.broadcastToRoom(playerInfo.roomId, null, {
                                    type: 'SCORE_UPDATE',
                                    scores: room.scores
                                });

                                // NUEVO: Victoria automática por puntos (mismo límite que en Unity: 3)
                                if (room.scores[winnerId] >= 3) {
                                    console.log(`¡${winnerId} ha ganado por alcanzar los 3 puntos!`);
                                    if (this.scoreService) {
                                        try {
                                            await this.scoreService.savePlayerScore(playerInfo.roomId, winnerId, 0, 3);
                                            this.broadcastToRoom(playerInfo.roomId, null, {
                                                type: 'GAME_OVER',
                                                winner: winnerId,
                                                scores: room.scores
                                            });
                                        } catch (e) {
                                            console.error("Error guardando victoria automática:", e);
                                        }
                                    }
                                }
                            }
                        }
                        // ---------------------------------------------

                        this.broadcastToRoom(playerInfo.roomId, ws, data);
                    } else {
                        this.broadcast(ws, data);
                    }
                    break;
                }
                case 'GAME_WON': {
                    console.log('Mensaje recibido: GAME_WON');
                    if (this.scoreService && playerInfo.roomId) {
                        try {
                            await this.scoreService.savePlayerScore(playerInfo.roomId, data.playerId, 0, 3);
                            console.log(`Puntuació de victòria desada per a l'usuari ${data.playerId}`);

                            // Avisamos a todos que ha terminado con el estado final
                            const room = this.rooms.get(playerInfo.roomId);
                            this.broadcastToRoom(playerInfo.roomId, null, {
                                type: 'GAME_OVER',
                                winner: data.playerId,
                                scores: room ? room.scores : {}
                            });
                        } catch (error) {
                            console.error("Error intentant guardar la puntuació:", error);
                        }
                    }
                    break;
                }
                case 'GET_ROOMS': {
                    const waitingRooms = await this.roomRepo.findWaitingRooms();
                    ws.send(JSON.stringify({
                        type: 'ROOMS_LIST',
                        rooms: waitingRooms.map(r => ({
                            id: r.id,
                            name: r.name,
                            playersCount: r.players ? r.players.length : 0,
                            maxPlayers: r.maxPlayers
                        }))
                    }));
                    break;
                }
                default:
                    console.log('Tipus de missatge desconegut:', data.type);
            }
        } catch (error) {
            console.error('Error processant el missatge del WebSocket:', error.message);
        }
    }

    async handleDisconnect(ws) {
        const playerInfo = this.clients.get(ws);
        if (playerInfo && playerInfo.roomId) {
            await this.leaveRoom(ws, playerInfo.roomId, playerInfo.playerId);
        }
        this.clients.delete(ws);
    }

    async leaveRoom(ws, roomId, playerId) {
        const room = this.rooms.get(roomId);
        if (room) {
            console.log(`Eliminando jugador ${playerId} de la sala ${roomId}`);
            room.removePlayer(playerId);
            await this.roomRepo.removePlayerFromDb(roomId, playerId);

            this.broadcastToRoom(roomId, ws, {
                type: 'PLAYER_LEFT',
                playerId: playerId
            });

            if (room.players.length === 0) {
                this.rooms.delete(roomId);
                await this.roomRepo.updateStatus(roomId, 'FINISHED');
                console.log(`Sala ${roomId} tancada y guardada como FINISHED.`);
            }
        }
    }

    broadcast(senderWs, data) {
        const messageString = JSON.stringify(data);
        this.wss.clients.forEach((client) => {
            if (client !== senderWs && client.readyState === WebSocket.OPEN) {
                client.send(messageString);
            }
        });
    }

    broadcastToRoom(roomId, senderWs, data) {
        const messageString = JSON.stringify(data);
        this.wss.clients.forEach((client) => {
            const clientInfo = this.clients.get(client);
            if (
                client !== senderWs &&
                client.readyState === WebSocket.OPEN &&
                clientInfo &&
                clientInfo.roomId === roomId
            ) {
                client.send(messageString);
            }
        });
    }
}

module.exports = SocketServer;