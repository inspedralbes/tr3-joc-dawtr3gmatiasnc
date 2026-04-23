# Memòria d'Aprenentatges i Problemes. Projecte Survivor
**Curs:** 2DAM 2025-2026 | **Autor:** [Matias Negrón Carranza]

## 1. Aprenentatges i Coses Interessants
Durant aquest projecte he consolidat una gran varietat de coneixements tècnics:
- **Arquitectura Hexagonal:** Implementació al backend en Node.js, separant la lògica de domini de la infraestructura (MongoDB, WebSockets). He treballat els Casos d'Ús i Repositoris.
- **IA Avançada i Comportaments:** Ús de Unity ML-Agents per entrenar enemics capaços de prendre decisions i prioritzar objectius (ex: atacar el jugador si entra al seu radi d'acció, en lloc de només centrar-se en el Trono).
- **Multijugador en Temps Real:** Gestió de sales en xarxa (Rooms) i sincronització de coordenades contínues, direcció de la mirada (dirX, dirY) i atacs de jugadors utilitzant el protocol de comunicació WebSockets de manera nativa (Sense llibreries externes com Socket.io al client).

## 2. Gestió de Tasques
La gestió i l'organització del projecte s'han dut a terme fraccionant les necessitats globals en petites fases (com documentava als meus plans interns, ex: `specs/plan.md`), avançant de forma iterativa i incremental:
1. **Fase de Fonaments i Disseny:** Crear els diagrames estructurals clau (Casos d'Ús, ERD, Seqüència i Arquitectura) per tenir clar el full de ruta i l'objectiu del codi final.
2. **Fase de Prototipat i IA:** Implementar la primera base en Unity i aconseguir que el sistema ML-Agents funcionés en el comportament enemic de manera autònoma.
3. **Fase d'Arquitectura i Sincronització Backend:** Crear el servidor de Node.js amb Arquitectura Hexagonal. Dividir-lo en Domini i Infraestructura. Validar connexions inicials.
4. **Fase de Resolució de Bugs i Persistència:** Detectar els errors de connexió (desincronització de WebSockets) al validar la part multijugador PvP, i acabar connectant els repositoris en memòria amb persistència real a MongoDB.

## 3. Problemes i Solucions (Problema / Tractament / Solució)

### Problema 1: Gestió global vs Aïllament de sales en el servidor WebSocket
**Problema:** En els primers passos del servidor, els missatges de moviment o d'atac s'enviaven a tots els jugadors que estiguessin connectats al mateix temps de forma indiscriminada (Broadcast Global). Això feia que jugadors de diferents instàncies de partides interferissin entre ells i els seus personatges es moguessin caòticament a les pantalles equivocades.
**Tractament:** Implementar una lògica estricta de "Sales" (Rooms). Calia associar cada connexió específica (`ws`) a un identificador de partida (`roomId`) i emetre informació de manera encapsulada.
**Solució:** Vaig establir un mapa en memòria temporal (`this.clients = new Map()`) per guardar a quina sala pertany cada usuari específic que es connecta, així com la seva posició de manera unívoca. Es va canviar el `broadcast` simple per la funció `broadcastToRoom`, que comprova la sala del client emissor i només en reenvia les dades a la resta de WebSockets pertanyents a la mateixa partida. També es va vincular això a MongoDB perquè les sales quedin marcades com a 'creades' i els jugadors persistits correctament.

### Problema 2: Desincronització i inconsistències d'estat en el combat (PvP i Atacs)
**Problema:** En els enfrontaments o enemics cooperatius, l'aplicació del dany de vegades fallava perquè els diversos clients calculaven la col·lisió de maneres diferents o amb lleuger retard. Quan moria un jugador es generaven estats invisibles i la càmara no inicialitzava de manera sincrònica i els clients no es podien veure de manera adient en fer *respawn*.
**Tractament:** Centrar l'autoritat d'esdeveniments. Treure l'autoritat que sigui l'atacat qui gestioni el dany cap a si mateix (ja que un retard de xarxa podria anul·lar-ho) i utilitzar un model de prioritat d'atacant. També calia notificar des del servidor que un esdeveniment ha tingut èxit per forçar a tots els clients a reaccionar, i finalment, desar el resultat a la base de dades sense sobrescriure's entre jugadors.
**Solució:** S'ha afegit el tipus de missatge `ATTACK` dins de l'estructura general i, per la victòria de la partida, el cas `GAME_WON`. D'aquesta manera, l'atac i els canvis grans passen pel filtre del servidor; quan algú venç, s'envia l'estat i llavors el servidor Node.js s'encarrega d'invocar el servei de persistència (`scoreService.savePlayerScore(playerInfo.roomId, data.playerId...`) actuant de font d'autoritat i de confiança per sincronitzar tots els estats multijugadors de forma neta evitant conflictes en la base de dades.

## 4. Explicació Detallada d'un Tros de Codi Interessant
Aquest és un fragment clau de la classe `SocketServer.js` de la carpeta `Infrastructure`, el qual forma el cor de connexió de WebSockets de la meva arquitectura. Gestiona què passa just en el moment en què un client Unity demana incorporar-se a una partida.

```javascript
case 'JOIN_ROOM': {
    const { roomId, playerId } = data;
    // 1. Busquem la sala prioritàriament a la memòria RAM del servidor
    let roomToJoin = this.rooms.get(roomId);

    // 2. Fallback de Persistència
    if (!roomToJoin) {
        const dbRoom = await this.roomRepo.findById(roomId);
        if (dbRoom) {
            // Re-instanciem en memòria només allò que necessitem
            roomToJoin = new Room(dbRoom.id, dbRoom.name, dbRoom.maxPlayers);
            if (dbRoom.players) {
                dbRoom.players.forEach(pId => roomToJoin.addPlayer(pId));
            }
            this.rooms.set(roomId, roomToJoin);
        } else {
            // Seguretat: Retornar al client que ha intentat unir-se a una sala que no existeix
            ws.send(JSON.stringify({ type: 'ERROR', message: 'La sala no existeix' }));
            return;
        }
    }

    // 3. Vinculem el Socket específic al jugador i el desem
    if (roomToJoin.addPlayer(playerId)) {
        // Inicialització en memòria del vector dirX i dirY
        this.clients.set(ws, { playerId, roomId, x: 0, y: 0, dirX: 1, dirY: 0 });
        
        // Cridem al cas d'ús/repositori pertinent per la BDD (Arquitectura Hexagonal)
        await this.roomRepo.addPlayerToDb(roomId, playerId);

        // 4. Distribució i Multi-avís de Connexió
        this.broadcastToRoom(roomId, ws, { type: 'PLAYER_JOINED', playerId });
        ws.send(JSON.stringify({ type: 'JOINED_ROOM', roomId }));
    } else {
        ws.send(JSON.stringify({ type: 'ERROR', message: 'La sala està plena o ja hi ets' }));
    }
    break;
}
```

### Anàlisi i per què és rellevant?
Aquest codi és potser dels més complexos pel que fa a l'organització d'estructures del servidor i val la pena destacar-ne els següents aspectes tècnics:

1. **Eficiència de Memòria vs Seguretat en Base de Dades (Caché local):** 
   A diferència d'aplicacions web tradicionals que farien *queries* constants a MongoDB (`find` / `update`) a cada petita interacció, en un joc en temps real l'accés a disc o base de dades és massa lent. Aquí s'ha programat de manera híbrida. El motor prioritza mirar al `Map()` intern de memòria volàtil (`this.rooms.get(roomId)`). Si el troba, serveix instantàniament. Només invoca una consulta a MongoDB via l'interface `roomRepo` si la sala ha deixat d'estar en memòria (per un reinici o *timeout*), garantint rapidesa mil·limètrica sense perdre dades en caigudes.
2. **Associació Dinàmica del Connexions Websocket Ocultes:** 
   El problema clàssic dels *Sockets* és que són "anònims". Per al node, un usuari només és una variable `ws`. El fragment `this.clients.set(ws, { playerId, roomId ... })` resol el puzle assignant tota la informació rellevant en una "estampa" vinculada a aquest socket.  Així la resta de mètodes, només agafant la variable `ws`, poden deduir de manera infal·lible en quina sala es troba i cap a on està mirant el client.
3. **Manteniment de l'Arquitectura Hexagonal en Temps Real:** 
   Fixa't en `await this.roomRepo.addPlayerToDb(roomId, playerId);`. Aquest tros de codi interactua directament entre el món dels protocols web constants (infraestructura websocket) i les funcions abstractes d'actualització. Així garanteixo que les mecàniques en xarxa s'han implementat paral·leles al principi de no acoblar bases de dades dins mateix del sistema de rutes, tal i com vam estudiar de Clean Code.
