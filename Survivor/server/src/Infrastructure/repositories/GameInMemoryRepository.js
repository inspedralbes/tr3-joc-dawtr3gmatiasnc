const IGameRepository = require('../../Domain/IGameRepository');

class GameInMemoryRepository extends IGameRepository {
    constructor() {
        super();
        this.games = new Map();
    }

    async create(gameData) {
        this.games.set(gameData.id, gameData);
        return gameData;
    }

    async finishGame(gameId) {
        if (!this.games.has(gameId)) return null;
        const game = this.games.get(gameId);
        game.status = 'FINISHED';
        this.games.set(gameId, game);
        return game;
    }
}
module.exports = GameInMemoryRepository;
