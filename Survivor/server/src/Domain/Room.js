class Room {
    constructor(id, name, maxPlayers = 4) {
        this.id = id;
        this.name = name;
        this.maxPlayers = maxPlayers;
        this.players = []; // Ids of players in this room
        this.status = 'WAITING'; // WAITING, PLAYING, FINISHED
    }

    addPlayer(playerId) {
        if (this.players.length < this.maxPlayers && !this.players.includes(playerId)) {
            this.players.push(playerId);
            return true;
        }
        return false;
    }

    removePlayer(playerId) {
        this.players = this.players.filter(id => id !== playerId);
    }

    isFull() {
        return this.players.length >= this.maxPlayers;
    }
}

module.exports = Room;
