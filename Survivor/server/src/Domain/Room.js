class Room {
    constructor(id, name, maxPlayers = 4) {
        this.id = id;
        this.name = name;
        this.maxPlayers = maxPlayers;
        this.players = []; // Ids of players in this room
        this.scores = {};  // playerId -> points
        this.status = 'WAITING'; // WAITING, PLAYING, FINISHED
    }

    addPlayer(playerId) {
        if (this.players.length < this.maxPlayers && !this.players.includes(playerId)) {
            this.players.push(playerId);
            if (!this.scores[playerId]) this.scores[playerId] = 0;
            return true;
        }
        return false;
    }

    removePlayer(playerId) {
        this.players = this.players.filter(id => id !== playerId);
        // Opcional: delete this.scores[playerId]; // Podem mantenir-ho si volem historial de la partida
    }

    updateScore(playerId, points) {
        if (this.scores[playerId] !== undefined) {
            this.scores[playerId] += points;
        } else {
            this.scores[playerId] = points;
        }
    }

    getRoomState() {
        return {
            id: this.id,
            name: this.name,
            players: this.players,
            scores: this.scores,
            status: this.status
        };
    }
}

module.exports = Room;
