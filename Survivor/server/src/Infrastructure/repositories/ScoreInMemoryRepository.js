const IScoreRepository = require('../../Domain/IScoreRepository');

class ScoreInMemoryRepository extends IScoreRepository {
    constructor() {
        super();
        this.scores = [];
    }

    async save(scoreData) {
        this.scores.push(scoreData);
        return scoreData;
    }

    async getLeaderboard(limit = 10) {
        // Ordena de major a menor
        const sorted = [...this.scores].sort((a, b) => b.survival_time_seconds - a.survival_time_seconds);
        return sorted.slice(0, limit);
    }
}
module.exports = ScoreInMemoryRepository;
