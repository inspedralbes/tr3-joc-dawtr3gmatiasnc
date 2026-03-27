const { getDb } = require('../database/db');

class UserMongoRepository {
    // Helper para no repetir getDb().collection('users') en cada función
    getCollection() {
        return getDb().collection('users');
    }

    async findById(id) {
        return await this.getCollection().findOne({ id: id });
    }

    async findByUsername(username) {
        return await this.getCollection().findOne({ username: username });
    }

    async save(user) {
        await this.getCollection().insertOne(user);
        return user;
    }
}

module.exports = UserMongoRepository;