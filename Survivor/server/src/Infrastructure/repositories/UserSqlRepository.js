const IUserRepository = require('../../Domain/IUserRepository');
const pool = require('../database/db'); 

class UserSqlRepository extends IUserRepository {
    async findById(id) {
        const [rows] = await pool.query('SELECT * FROM users WHERE id = ?', [id]);
        return rows[0] || null;
    }

    async findByUsername(username) {
        const [rows] = await pool.query('SELECT * FROM users WHERE username = ?', [username]);
        return rows[0] || null;
    }

    async save(user) {
        await pool.query(
            'INSERT INTO users (id, username, password_hash) VALUES (?, ?, ?)',
            [user.id, user.username, user.password_hash]
        );
        return user;
    }
}

module.exports = UserSqlRepository;