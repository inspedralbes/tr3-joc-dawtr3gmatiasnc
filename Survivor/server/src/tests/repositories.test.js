// Aquest arxiu serveix per demostrar el requisit de la rúbrica: 
// "Tests unitaris dels Repositories amb implementació InMemory"

const UserInMemoryRepository = require('../Infrastructure/repositories/UserInMemoryRepository');
const GameInMemoryRepository = require('../Infrastructure/repositories/GameInMemoryRepository');
const ScoreInMemoryRepository = require('../Infrastructure/repositories/ScoreInMemoryRepository');
const assert = require('assert');

async function runTests() {
    console.log("Iniciant tests unitaris dels Repositories InMemory...\n");

    // TEST 1: UserInMemoryRepository
    const userRepo = new UserInMemoryRepository();
    const mockUser = { id: 'u1', username: 'Matias', password_hash: '123' };
    await userRepo.save(mockUser);
    const foundUser = await userRepo.findByUsername('Matias');
    assert.strictEqual(foundUser.username, 'Matias', "L'usuari hauria d'existir");
    console.log("✅ UserInMemoryRepository Test Superat");

    // TEST 2: GameInMemoryRepository
    const gameRepo = new GameInMemoryRepository();
    const mockGame = { id: 'g1', status: 'PLAYING' };
    await gameRepo.create(mockGame);
    await gameRepo.finishGame('g1');
    const finishedGame = gameRepo.games.get('g1');
    assert.strictEqual(finishedGame.status, 'FINISHED', "La partida hauria de canviar a estat FINISHED");
    console.log("✅ GameInMemoryRepository Test Superat");

    // TEST 3: ScoreInMemoryRepository
    const scoreRepo = new ScoreInMemoryRepository();
    const mockScore1 = { id: 's1', survival_time_seconds: 100 };
    const mockScore2 = { id: 's2', survival_time_seconds: 500 };
    await scoreRepo.save(mockScore1);
    await scoreRepo.save(mockScore2);
    const leaderboard = await scoreRepo.getLeaderboard(1);
    assert.strictEqual(leaderboard[0].survival_time_seconds, 500, "El primer lloc hauria de ser la puntuació més alta");
    console.log("✅ ScoreInMemoryRepository Test Superat");

    console.log("\n🎉 Tots els tests dels Repositories han passat amb èxit!");
}

runTests().catch(err => {
    console.error("❌ Error en els tests:", err);
});
