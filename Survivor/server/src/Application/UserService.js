const { v4: uuidv4 } = require('uuid');
const bcrypt = require('bcrypt');

class UserService {
    constructor(userRepository) {
        this.userRepository = userRepository;
    }

    async registerUser(username, plainPassword) {
        if (!username || !plainPassword) {
            throw new Error("Username and password are required.");
        }

        const existingUser = await this.userRepository.findByUsername(username);
        if (existingUser) {
            throw new Error("Username already taken.");
        }

        const hashedPassword = await bcrypt.hash(plainPassword, 10);
        const newUser = {
            id: uuidv4(),
            username: username,
            password_hash: hashedPassword
        };

        return await this.userRepository.save(newUser);
    }
    async loginUser(username, plainPassword) {
        if (!username || !plainPassword) {
            throw new Error("Es requereix nom d'usuari i contrasenya.");
        }

        const user = await this.userRepository.findByUsername(username);
        if (!user) {
            throw new Error("Credencials incorrectes.");
        }

        const isPasswordValid = await bcrypt.compare(plainPassword, user.password_hash);
        if (!isPasswordValid) {
            throw new Error("Credencials incorrectes.");
        }

        return {
            id: user.id,
            username: user.username
        };
    }
}

module.exports = UserService;