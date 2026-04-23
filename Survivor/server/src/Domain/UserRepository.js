class IUserRepository {
    async findById(id) {
        throw new Error("ERR_METHOD_NOT_IMPLEMENTED");
    }

    async findByUsername(username) {
        throw new Error("ERR_METHOD_NOT_IMPLEMENTED");
    }

    async save(user) {
        throw new Error("ERR_METHOD_NOT_IMPLEMENTED");
    }
}

module.exports = IUserRepository;