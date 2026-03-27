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
        return newScore;
    }

    async getLeaderboard(limit = 10) {
        // Creuem la col·lecció 'scores' amb 'users' igual que abans, però amb el driver natiu
        const cursor = await this.getCollection().aggregate([
            { $sort: { survival_time_seconds: -1 } }, // Ordenar de major a menor
            { $limit: limit }, // Top X
            {
                $lookup: {
                    from: 'users', // Taula (col·lecció) destí
                    localField: 'user_id', // El camp de 'scores'
                    foreignField: 'id', // El camp de 'users'
                    as: 'user_info'
                }
            },
            { $unwind: '$user_info' }, // Desempaquetar l'array creat pel lookup
            {
                $project: { // Seleccionar només allò que volem enviar a Unity
                    username: '$user_info.username',
                    survival_time_seconds: 1,
                    level_reached: 1,
                    created_at: 1,
                    _id: 0 // Amaguem l'ID intern de Mongo
                }
            }
        ]);

        // Convertim el cursor a un array abans de retornar-ho
        return await cursor.toArray();
    }
}

module.exports = ScoreMongoRepository;