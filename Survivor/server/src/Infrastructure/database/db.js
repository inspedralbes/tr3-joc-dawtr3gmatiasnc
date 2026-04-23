const { MongoClient } = require('mongodb');
require('dotenv').config();

const client = new MongoClient(process.env.MONGO_URI);
let db;

const connectMongo = async () => {
    try {
        const mongoUri = process.env.MONGO_URI || 'mongodb://localhost:27017';
        const dbName = process.env.DB_NAME || 'JocMatias';

        // Masked URI for safe logging
        const maskedUri = mongoUri.replace(/\/\/.*@/, "//****:****@");
        console.log(`Intentando conectar a MongoDB: ${maskedUri}`);
        console.log(`Base de datos objetivo: ${dbName}`);

        await client.connect();
        db = client.db(dbName);
        console.log('¡Conexión exitosa a MongoDB!');
    } catch (error) {
        console.error('Error fatal al conectar a MongoDB:', error.message);
        process.exit(1);
    }
};

const getDb = () => db;

module.exports = { connectMongo, getDb };