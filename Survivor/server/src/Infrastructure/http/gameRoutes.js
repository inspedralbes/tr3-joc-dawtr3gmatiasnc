const express = require('express');
const router = express.Router();

module.exports = (gameController) => {
    // POST /api/games -> Inicia una nova partida
    router.post('/', gameController.start.bind(gameController));

    return router;
};