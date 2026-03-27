const WebSocket = require('ws');

class SocketServer {
    constructor(server) {
        // Inicialitzem el servidor WebSocket vinculat al servidor HTTP existent
        this.wss = new WebSocket.Server({ server });

        // Un Map per guardar els clients connectats i la seva informació (ex: coordenades)
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
            // Unity ens enviarà missatges en format JSON
            const data = JSON.parse(message);

            switch (data.type) {
                case 'JOIN':
                    // Registrem el jugador quan entra a la partida
                    this.clients.set(ws, { playerId: data.playerId, x: 0, y: 0 });
                    console.log(`Jugador ${data.playerId} s'ha unit a l'arena.`);
                    break;
                case 'MOVE':
                    // Actualitzem l'estat intern al servidor
                    const player = this.clients.get(ws);
                    if (player) {
                        player.x = data.x;
                        player.y = data.y;
                    }
                    // Retransmetem aquest moviment a la resta de jugadors
                    this.broadcast(ws, data);
                    break;
                default:
                    console.log('Tipus de missatge desconegut:', data.type);
            }
        } catch (error) {
            console.error('Error processant el missatge del WebSocket:', error.message);
        }
    }

    // Funció per enviar missatges a TOTS els clients EXCEPTE al que l'ha enviat
    broadcast(senderWs, data) {
        const messageString = JSON.stringify(data);
        this.wss.clients.forEach((client) => {
            // Comprovem que el client no sigui l'emissor i que la connexió estigui oberta
            if (client !== senderWs && client.readyState === WebSocket.OPEN) {
                client.send(messageString);
            }
        });
    }
}

module.exports = SocketServer;