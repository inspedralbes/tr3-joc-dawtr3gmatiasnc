const { MongoClient } = require('mongodb');
require('dotenv').config();

const client = new MongoClient(process.env.MONGO_URI);
let db;

const connectMongo = async () => {
    try {
        await client.connect();
        // Usamos el nombre de la DB del .env o 'neon_survivor' por defecto
        db = client.db(process.env.DB_NAME || 'JocMatias');
        console.log('Conectado a MongoDB');
    } catch (error) {
        console.error('Error fatal al conectar a MongoDB:', error.message);
        process.exit(1);
    }
};

// Función para obtener la instancia de la base de datos desde los repositorios
const getDb = () => db;

module.exports = { connectMongo, getDb };