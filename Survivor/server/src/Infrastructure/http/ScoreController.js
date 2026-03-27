class ScoreController {
    constructor(scoreService) {
        this.scoreService = scoreService;
    }

    async saveScore(req, res) {
        try {
            const { gameId, userId, survivalTime, levelReached } = req.body;
            const score = await this.scoreService.savePlayerScore(gameId, userId, survivalTime, levelReached);
            res.status(201).json(score);
        } catch (error) {
            res.status(500).json({ error: error.message });
        }
    }

    async getLeaderboard(req, res) {
        try {
            const leaderboard = await this.scoreService.getTopPlayers();
            res.status(200).json(leaderboard);
        } catch (error) {
            res.status(500).json({ error: error.message });
        }
    }
}

module.exports = ScoreController;