const express = require('express');
const router = express.Router();

module.exports = (scoreController) => {
    router.post('/', scoreController.saveScore.bind(scoreController));

    router.get('/leaderboard', scoreController.getLeaderboard.bind(scoreController));

    return router;
};