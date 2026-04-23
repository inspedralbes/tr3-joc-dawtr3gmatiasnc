class GameController {
    constructor(gameService) {
        this.gameService = gameService;
    }

    async start(req, res) {
        try {
            const game = await this.gameService.startGame();
            res.status(201).json(game);
        } catch (error) {
            res.status(500).json({ error: error.message });
        }
    }
}

module.exports = GameController;