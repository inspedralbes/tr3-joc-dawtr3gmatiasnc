const express = require('express');
const router = express.Router();

module.exports = (gameController) => {
    router.post('/', gameController.start.bind(gameController));

    return router;
};