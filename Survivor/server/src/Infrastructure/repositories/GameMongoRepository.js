const { getDb } = require('../database/db');

class GameMongoRepository {
    // Helper per accedir ràpidament a la col·lecció
    getCollection() {
        return getDb().collection('games');
    }

    async create(gameData) {
        // Assegurem que l'estat i les dates per defecte hi siguin
        const newGame = {
            id: gameData.id,
            status: gameData.status || 'WAITING',
            started_at: new Date(),
            ended_at: null
        };

        await this.getCollection().insertOne(newGame);
        return newGame;
    }

    async finishGame(id) {
        // Actualitzem només els camps necessaris amb $set
        await this.getCollection().updateOne(
            { id: id },
            { $set: { status: 'FINISHED', ended_at: new Date() } }
        );
    }
}

module.exports = GameMongoRepository;