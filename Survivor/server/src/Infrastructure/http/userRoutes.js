const express = require('express');
const router = express.Router();

module.exports = (userController) => {


    router.post('/register', userController.register.bind(userController));

    router.post('/login', userController.login.bind(userController));

    return router;
};
















