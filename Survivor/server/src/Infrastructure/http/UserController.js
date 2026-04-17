class UserController {
    constructor(userService) {
        this.userService = userService;
    }

    async register(req, res) {
        try {
            const { username, password } = req.body;

            const newUser = await this.userService.registerUser(username, password);

            res.status(201).json({
                message: "User registered successfully",
                user: { id: newUser.id, username: newUser.username }
            });
        } catch (error) {
            res.status(400).json({ error: error.message });
        }
    }

    async login(req, res) {
        try {
            const { username, password } = req.body;

            const user = await this.userService.loginUser(username, password);

            res.status(200).json({
                message: "Login correcte",
                user: user
            });
        } catch (error) {
            res.status(401).json({ error: error.message });
        }
    }
}

module.exports = UserController;