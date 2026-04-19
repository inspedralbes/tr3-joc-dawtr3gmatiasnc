# Survivor Multijugador

**Nom dels integrants:** Matias  
**Nom del projecte:** Survivor Multijugador  

**Petita descripció:**  
Videojoc 2D d'acció multijugador desenvolupat en Unity. Utilitza una arquitectura Client-Servidor on el backend està programat amb Node.js (Express i WebSockets) seguint Arquitectura Hexagonal. Els jugadors poden iniciar sessió, crear sales, i lluitar entre ells en temps real. A més, inclou IA entrenada amb ML-Agents i persistència de dades (usuaris, partides i puntuacions) en una base de dades MongoDB.

**URL de producció:**  
*(Posa aquí l'enllaç del joc un cop el pugis a producció / WebGL, o "En entorn local de moment")*

**Estat:**  
**Finalitzat.** S'ha completat la integració total entre el client (Unity) i el backend (Node.js). Els sockets funcionen per moure's i atacar. El flux de partides (crear, unir-se, guanyar i guardar el resultat a MongoDB) està plenament operatiu. També s'hi ha afegit el mode solitari amb enemics IA.
