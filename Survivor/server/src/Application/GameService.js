const { v4: uuidv4 } = require('uuid');

class GameService {
    constructor(gameRepository) {
        this.gameRepository = gameRepository;
    }

    // Lógica para empezar una partida
    async startGame() {
        const newGame = {
            id: uuidv4(),
            status: 'PLAYING',
            started_at: new Date()
        };

        // Llamamos al repositorio para guardarlo en Mongo
        return await this.gameRepository.create(newGame);
    }

    // Lógica para terminar una partida
    async endGame(gameId) {
        if (!gameId) {
            throw new Error("Se requiere el ID de la partida para finalizarla.");
        }

        return await this.gameRepository.finishGame(gameId);
    }
}

module.exports = GameService;