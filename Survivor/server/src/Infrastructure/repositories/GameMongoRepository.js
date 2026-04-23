const { getDb } = require('../database/db');

class GameMongoRepository {
    getCollection() {
        return getDb().collection('games');
    }

    async create(gameData) {
        const newGame = {
            id: gameData.id,
            status: gameData.status || 'WAITING',
            started_at: new Date(),
            ended_at: null
        };

        await this.getCollection().insertOne(newGame);
        console.log(`[Mongo] Partida registrada con éxito: ${newGame.id}`);
        return newGame;
    }

    async finishGame(id) {
        await this.getCollection().updateOne(
            { id: id },
            { $set: { status: 'FINISHED', ended_at: new Date() } }
        );
    }
}

module.exports = GameMongoRepository;