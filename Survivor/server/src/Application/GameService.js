const { v4: uuidv4 } = require('uuid');

class GameService {
    constructor(gameRepository) {
        this.gameRepository = gameRepository;
    }

    async startGame(gameId = null) {
        const newGame = {
            id: gameId || uuidv4(),
            status: 'PLAYING',
            started_at: new Date()
        };

        return await this.gameRepository.create(newGame);
    }

    async endGame(gameId) {
        if (!gameId) {
            throw new Error("Se requiere el ID de la partida para finalizarla.");
        }

        return await this.gameRepository.finishGame(gameId);
    }
}

module.exports = GameService;