const WebSocket = require('ws');
const Room = require('../../Domain/Room');
const RoomMongoRepository = require('../repositories/RoomMongoRepository');

class SocketServer {
    constructor(server) {
        this.wss = new WebSocket.Server({ server });
        
        this.clients = new Map(); // ws -> { playerId, roomId, x, y }
        this.rooms = new Map();   // roomId -> Room instance
        this.roomRepo = new RoomMongoRepository();

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

    async handleMessage(ws, message) {
        try {
            const data = JSON.parse(message);
            const playerInfo = this.clients.get(ws) || {};

            switch (data.type) {
                case 'CREATE_ROOM': {
                    const newRoomId = data.roomId || `room-${Date.now()}`;
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
                    
                    ws.send(JSON.stringify({ type: 'ROOM_CREATED', roomId: newRoomId }));
                    console.log(`Sala creada: ${newRoomId}`);
                    break;
                }
                case 'JOIN_ROOM': {
                    const { roomId, playerId } = data;
                    let roomToJoin = this.rooms.get(roomId);
                    
                    if (!roomToJoin) {
                        // Buscar en BD si existe y cargarla a memoria
                        const dbRoom = await this.roomRepo.findById(roomId);
                        if (dbRoom) {
                            roomToJoin = new Room(dbRoom.id, dbRoom.name, dbRoom.maxPlayers);
                            // Restaurar los jugadores actuales (si hay en DB)
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
                        this.clients.set(ws, { playerId, roomId, x: 0, y: 0 });
                        await this.roomRepo.addPlayerToDb(roomId, playerId);
                        
                        console.log(`Jugador ${playerId} s'ha unit a la sala ${roomId}.`);
                        this.broadcastToRoom(roomId, ws, { type: 'PLAYER_JOINED', playerId });
                        ws.send(JSON.stringify({ type: 'JOINED_ROOM', roomId }));
                    } else {
                        ws.send(JSON.stringify({ type: 'ERROR', message: 'La sala està plena o ja hi ets' }));
                    }
                    break;
                }
                case 'MOVE': {
                    if (playerInfo.roomId) {
                        playerInfo.x = data.x;
                        playerInfo.y = data.y;
                        this.clients.set(ws, playerInfo);
                        
                        // Solo enviar el movimiento a los de la misma sala
                        this.broadcastToRoom(playerInfo.roomId, ws, data);
                    } else {
                        // Por compatibilidad temporal si mandan MOVE sin sala
                        this.broadcast(ws, data);
                    }
                    break;
                }
                case 'GET_ROOMS': {
                    // El cliente solicita la lista de salas disponibles
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
            const room = this.rooms.get(playerInfo.roomId);
            if (room) {
                room.removePlayer(playerInfo.playerId);
                await this.roomRepo.removePlayerFromDb(playerInfo.roomId, playerInfo.playerId);
                
                this.broadcastToRoom(playerInfo.roomId, ws, { 
                    type: 'PLAYER_LEFT', 
                    playerId: playerInfo.playerId 
                });
                
                // Si la sala se queda vacía, la eliminamos de memoria (opcional)
                if (room.players.length === 0) {
                    this.rooms.delete(playerInfo.roomId);
                    await this.roomRepo.updateStatus(playerInfo.roomId, 'FINISHED');
                    console.log(`Sala ${playerInfo.roomId} tancada per inactivitat.`);
                }
            }
        }
        this.clients.delete(ws);
    }

    // Broadcast global (se mantiene por si acaso)
    broadcast(senderWs, data) {
        const messageString = JSON.stringify(data);
        this.wss.clients.forEach((client) => {
            if (client !== senderWs && client.readyState === WebSocket.OPEN) {
                client.send(messageString);
            }
        });
    }

    // Broadcast a los clientes de una sala específica
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