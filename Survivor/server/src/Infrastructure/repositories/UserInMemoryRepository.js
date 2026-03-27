const IUserRepository = require('../../Domain/IUserRepository');

class UserInMemoryRepository extends IUserRepository {
    constructor() {
        super();
        this.users = new Map();
    }

    async findById(id) {
        return this.users.get(id) || null;
    }

    async findByUsername(username) {
        for (let [id, user] of this.users) {
            if (user.username === username) return user;
        }
        return null;
    }

    async save(user) {
        this.users.set(user.id, user);
        return user;
    }
}

module.exports = UserInMemoryRepository;