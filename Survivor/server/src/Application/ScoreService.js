const { v4: uuidv4 } = require('uuid');

class ScoreService {
    constructor(scoreRepository, gameService) {
        this.scoreRepository = scoreRepository;
        this.gameService = gameService; 
    }

    async savePlayerScore(gameId, userId, survivalTime, levelReached) {
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

        const savedScore = await this.scoreRepository.save(newScore);

        await this.gameService.endGame(gameId);

        return savedScore;
    }

    async getTopPlayers() {
        return await this.scoreRepository.getLeaderboard(10);
    }
}

module.exports = ScoreService;