const { v4: uuidv4 } = require('uuid');

class ScoreService {
    constructor(scoreRepository, gameService) {
        this.scoreRepository = scoreRepository;
        this.gameService = gameService; // Lo necesitamos para cerrar la partida al guardar el score
    }

    // Lógica para guardar la puntuación cuando el jugador muere
    async savePlayerScore(gameId, userId, survivalTime, levelReached) {
        // REGLAS DE NEGOCIO: Validaciones básicas
        if (!gameId || !userId) throw new Error("Datos de partida o usuario incompletos.");
        if (survivalTime < 0) throw new Error("El tiempo de supervivencia no puede ser negativo.");

        const newScore = {
            id: uuidv4(),
            game_id: gameId,
            user_id: userId,
            survival_time_seconds: survivalTime,
            level_reached: levelReached,
            created_at: new Date()
        };

        // 1. Guardamos la puntuación en MongoDB
        const savedScore = await this.scoreRepository.save(newScore);

        // 2. Cerramos la partida automáticamente
        await this.gameService.endGame(gameId);

        return savedScore;
    }

    // Lógica para obtener el ranking mundial (Top 10)
    async getTopPlayers() {
        return await this.scoreRepository.getLeaderboard(10);
    }
}

module.exports = ScoreService;