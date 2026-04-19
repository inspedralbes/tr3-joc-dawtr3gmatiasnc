# Context
L'aplicació és un joc tipus "Survivor" (Singleplayer) on els enemics estan controlats per Intel·ligència Artificial utilitzant Unity ML-Agents (`EnemigoAgente.cs`). Actualment, els enemics tenen un únic objectiu estàtic: buscar, apropar-se i destruir el "Trono".

# Problema
El comportament de l'enemic és massa previsible i lineal. Només es dirigeix cap al Trono ignorant completament el Jugador, fins i tot si aquest se li posa al davant per atacar-lo o defensar el trono. Això fa que l'experiència de joc sigui poc desafiant i mecànica, ja que el jugador no representa una amenaça real per l'atenció de la IA.

# Objectius
- Implementar un sistema d'"Aggro" (Prioritat Dinàmica d'Objectius) per a l'Agent.
- Si el Jugador s'apropa a l'enemic dins d'un radi de detecció específic, la IA ha de canviar el seu objectiu i començar a perseguir i atacar el Jugador en comptes del Trono.
- Si el Jugador s'allunya prou o surt del radi, la IA ha de reprendre el seu camí original cap al Trono automàticament.

# Restriccions
- El sistema ha d'estar completament integrat dins de `EnemigoAgente.cs` sense dependre de plugins o scripts de tercers.
- L'agent d'ML-Agents ha de continuar utilitzant la mateixa quantitat d'observacions de vectors al mètode `CollectObservations`, per evitar haver de reentrenar de zero tota la xarxa neuronal del cervell a Unity. L'enganyarem canviant el destí internament.
