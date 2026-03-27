const express = require('express');
const router = express.Router();

// Esta función recibe el controlador ya configurado desde index.js
module.exports = (userController) => {

    // Vincular la ruta POST /register al método del controlador
    // Usamos .bind(userController) para no perder el contexto de "this"
    router.post('/register', userController.register.bind(userController));

    router.post('/login', userController.login.bind(userController));

    return router;
};