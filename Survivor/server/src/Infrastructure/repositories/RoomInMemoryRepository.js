const IRoomRepository = require('../../Domain/IRoomRepository');

class RoomInMemoryRepository extends IRoomRepository {
    constructor() {
        super();
        this.rooms = new Map();
    }

    async create(roomData) {
        this.rooms.set(roomData.id, roomData);
        return roomData;
    }

    async findById(roomId) {
        return this.rooms.get(roomId) || null;
    }

    async findWaitingRooms() {
        const waitingRooms = [];
        for (const room of this.rooms.values()) {
            if (room.status === 'WAITING') {
                waitingRooms.push(room);
            }
        }
        return waitingRooms;
    }

    async addPlayerToDb(roomId, playerId) {
        const room = this.rooms.get(roomId);
        if (room) {
            if (!room.players) room.players = [];
            room.players.push(playerId);
            this.rooms.set(roomId, room);
        }
    }

    async removePlayerFromDb(roomId, playerId) {
        const room = this.rooms.get(roomId);
        if (room && room.players) {
            room.players = room.players.filter(p => p !== playerId);
            this.rooms.set(roomId, room);
        }
    }

    async updateStatus(roomId, newStatus) {
        const room = this.rooms.get(roomId);
        if (room) {
            room.status = newStatus;
            this.rooms.set(roomId, room);
        }
    }

    async deleteRoom(roomId) {
        this.rooms.delete(roomId);
    }
}
module.exports = RoomInMemoryRepository;
