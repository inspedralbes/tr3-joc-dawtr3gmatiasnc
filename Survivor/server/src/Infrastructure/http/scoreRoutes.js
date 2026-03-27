const express = require('express');
const router = express.Router();

module.exports = (scoreController) => {
    // POST /api/scores -> Guarda la puntuació final i tanca la partida
    router.post('/', scoreController.saveScore.bind(scoreController));

    // GET /api/scores/leaderboard -> Retorna el Top 10
    router.get('/leaderboard', scoreController.getLeaderboard.bind(scoreController));

    return router;
};