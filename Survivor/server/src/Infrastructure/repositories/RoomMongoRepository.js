const IRoomRepository = require('../../Domain/IRoomRepository');
const { getDb } = require('../database/db');

class RoomMongoRepository extends IRoomRepository {
    getCollection() {
        return getDb().collection('rooms');
    }

    async create(roomData) {
        const newRoom = {
            id: roomData.id,
            name: roomData.name,
            maxPlayers: roomData.maxPlayers || 4,
            players: roomData.players || [],
            status: roomData.status || 'WAITING',
            created_at: new Date()
        };

        await this.getCollection().insertOne(newRoom);
        return newRoom;
    }

    async findById(id) {
        return await this.getCollection().findOne({ id: id });
    }

    async findWaitingRooms() {
        // Devuelve las salas que están en estado WAITING
        return await this.getCollection().find({ status: 'WAITING' }).toArray();
    }

    async updateStatus(id, status) {
        await this.getCollection().updateOne(
            { id: id },
            { $set: { status: status, updated_at: new Date() } }
        );
    }
    
    async addPlayerToDb(id, playerId) {
        await this.getCollection().updateOne(
            { id: id },
            { $addToSet: { players: playerId } }
        );
    }

    async removePlayerFromDb(id, playerId) {
        await this.getCollection().updateOne(
            { id: id },
            { $pull: { players: playerId } }
        );
    }
}

module.exports = RoomMongoRepository;
