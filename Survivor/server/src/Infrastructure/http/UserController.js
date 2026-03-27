class UserController {
    // Inyectamos el servicio para que el controlador no sepa nada de bases de datos
    constructor(userService) {
        this.userService = userService;
    }

    // Método para el endpoint POST /api/users/register
    async register(req, res) {
        try {
            const { username, password } = req.body;

            // Llamamos a la lógica de negocio que creamos en el paso anterior
            const newUser = await this.userService.registerUser(username, password);

            // Si todo va bien, devolvemos un 201 (Created) sin la contraseña hasheada
            res.status(201).json({
                message: "User registered successfully",
                user: { id: newUser.id, username: newUser.username }
            });
        } catch (error) {
            // Si el usuario ya existe o faltan datos, devolvemos un 400 (Bad Request)
            res.status(400).json({ error: error.message });
        }
    }

    // Método para el endpoint POST /api/users/login (Estructura base)
    async login(req, res) {
        try {
            const { username, password } = req.body;

            // Executem el cas d'ús de login
            const user = await this.userService.loginUser(username, password);

            // Si tot va bé, enviem un HTTP 200 (OK)
            res.status(200).json({
                message: "Login correcte",
                user: user
            });
        } catch (error) {
            // HTTP 401 (Unauthorized) si l'usuari o la contrasenya fallen
            res.status(401).json({ error: error.message });
        }
    }
}

module.exports = UserController;