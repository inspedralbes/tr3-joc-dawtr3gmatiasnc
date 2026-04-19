class IScoreRepository {
    async save(scoreData) { throw new Error('Not implemented'); }
    async getLeaderboard(limit) { throw new Error('Not implemented'); }
}
module.exports = IScoreRepository;
