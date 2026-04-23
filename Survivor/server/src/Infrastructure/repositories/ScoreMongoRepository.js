const { getDb } = require('../database/db');

class ScoreMongoRepository {
    getCollection() {
        return getDb().collection('scores');
    }

    async save(scoreData) {
        const newScore = {
            id: scoreData.id,
            game_id: scoreData.game_id,
            user_id: scoreData.user_id,
            survival_time_seconds: scoreData.survival_time_seconds || 0,
            level_reached: scoreData.level_reached || 1,
            created_at: new Date()
        };

        await this.getCollection().insertOne(newScore);
        console.log(`[Mongo] Puntuación guardada con éxito para el usuario: ${newScore.user_id}`);
        return newScore;
    }

    async getLeaderboard(limit = 10) {
        const cursor = await this.getCollection().aggregate([
            { $sort: { survival_time_seconds: -1 } },
            { $limit: limit },
            {
                $lookup: {
                    from: 'users',
                    localField: 'user_id',
                    foreignField: 'id',
                    as: 'user_info'
                }
            },
            { $unwind: '$user_info' },
            {
                $project: {
                    username: '$user_info.username',
                    survival_time_seconds: 1,
                    level_reached: 1,
                    created_at: 1,
                    _id: 0
                }
            }
        ]);

        return await cursor.toArray();
    }
}

module.exports = ScoreMongoRepository;