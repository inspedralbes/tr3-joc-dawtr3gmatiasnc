const WebSocket = require('ws');

class SocketServer {
    constructor(server) {
        this.wss = new WebSocket.Server({ server });

        this.clients = new Map();

        this.wss.on('connection', (ws) => {
            console.log('Nou jugador connectat al WebSocket!');

            ws.on('message', (message) => {
                this.handleMessage(ws, message);
            });

            ws.on('close', () => {
                console.log('Jugador desconnectat del WebSocket');
                this.clients.delete(ws);
            });
        });
    }

    handleMessage(ws, message) {
        try {
            const data = JSON.parse(message);

            switch (data.type) {
                case 'JOIN':
                    this.clients.set(ws, { playerId: data.playerId, x: 0, y: 0 });
                    console.log(`Jugador ${data.playerId} s'ha unit a l'arena.`);
                    break;
                case 'MOVE':
                    const player = this.clients.get(ws);
                    if (player) {
                        player.x = data.x;
                        player.y = data.y;
                    }
                    this.broadcast(ws, data);
                    break;
                default:
                    console.log('Tipus de missatge desconegut:', data.type);
            }
        } catch (error) {
            console.error('Error processant el missatge del WebSocket:', error.message);
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
}

module.exports = SocketServer;