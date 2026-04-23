# Comportament Esperat (Spec)

## 1. Dades de l'Agent (`EnemigoAgente.cs`)
S'afegeixen noves variables per gestionar la presència del jugador a l'entorn:
- `public Transform transformJugador;`
- `public float radiDeteccio = 5f;`
- `private Transform objectiuActual;` (Ens servirà per saber si estem apuntant al Trono o al Jugador en aquest frame).

## 2. Detecció d'Objectius (Lògica d'Aggro)
A cada frame on l'agent hagi de prendre una decisió (abans de recollir dades per a la xarxa neuronal), l'agent calcularà la distància fins al `transformJugador`:
- Si la distància entre l'enemic i el jugador és menor o igual al `radiDeteccio`, llavors `objectiuActual = transformJugador`.
- Si el jugador està massa lluny, `objectiuActual = objectiu` (el Trono per defecte).

## 3. Observacions i Recompenses de ML-Agents
- A `CollectObservations`, el vector de direcció es calcularà sempre cap al `objectiuActual`, ja no cap al Trono de manera directa. Aquest és el punt clau per no haver de reentrenar la IA de zero.
- A `OnActionReceived`, les recompenses (`AddReward(+0.001f)`) avaluaran la distància envers l'objectiu actual.

## 4. Sistema d'Atac i Col·lisions
S'afegeix un nou comportament de col·lisió:
- Si hi ha un `OnCollisionStay2D` contra l'etiqueta "Player":
  - Es reprodueix l'animació "Atacando".
  - Passat el cooldown (`velocidadAtaque`), es restarà vida al jugador invocant un hipotètic component `VidaJugador`.
- Si hi ha un `OnCollisionExit2D` contra "Player", s'apaga l'animació.
