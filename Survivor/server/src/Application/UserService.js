const { v4: uuidv4 } = require('uuid');
const bcrypt = require('bcrypt');

class UserService {
    // Inyectamos el repositorio por el constructor
    constructor(userRepository) {
        this.userRepository = userRepository;
    }

    async registerUser(username, plainPassword) {
        // 1. Regla de negocio: Validar datos básicos
        if (!username || !plainPassword) {
            throw new Error("Username and password are required.");
        }

        // 2. Regla de negocio: Comprobar si el usuario ya existe
        const existingUser = await this.userRepository.findByUsername(username);
        if (existingUser) {
            throw new Error("Username already taken.");
        }

        // 3. Crear el usuario
        const hashedPassword = await bcrypt.hash(plainPassword, 10);
        const newUser = {
            id: uuidv4(),
            username: username,
            password_hash: hashedPassword
        };

        // 4. Guardar usando el repositorio abstraído
        return await this.userRepository.save(newUser);
    }
    // Afegeix això dins de la classe UserService
    async loginUser(username, plainPassword) {
        // 1. Validar dades d'entrada
        if (!username || !plainPassword) {
            throw new Error("Es requereix nom d'usuari i contrasenya.");
        }

        // 2. Buscar l'usuari utilitzant el repositori
        const user = await this.userRepository.findByUsername(username);
        if (!user) {
            // Retornem un error genèric per no donar pistes a possibles atacants
            throw new Error("Credencials incorrectes.");
        }

        // 3. Comparar la contrasenya introduïda amb el hash guardat a la DB
        const isPasswordValid = await bcrypt.compare(plainPassword, user.password_hash);
        if (!isPasswordValid) {
            throw new Error("Credencials incorrectes.");
        }

        // 4. Retornar les dades de l'usuari netejades (MAI retornar el hash de la contrasenya!)
        return {
            id: user.id,
            username: user.username
        };
    }
}

module.exports = UserService;