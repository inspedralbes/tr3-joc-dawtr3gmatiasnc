# Registre de Traçabilitat (Prompts Log) - Spec-Driven Development

## Funcionalitat: Sistema d'Aggro (Prioritat Dinàmica d'Objectius) per a ML-Agents (Singleplayer)

---

### Iteració 1: Definició de la funcionalitat i specs (opsx:propose)

**El meu Prompt:**
> "Tenint en compte el meu script actual `EnemigoAgente.cs` on un enemic ataca a un Trono utilitzant Unity ML-Agents en un mode de joc tipus Survivor (Singleplayer), vull afegir-hi una funcionalitat que el faci més intel·ligent. Vull un sistema d'Aggro: si el jugador s'apropa a menys de 5 unitats, l'enemic deixarà de perseguir el Trono i anirà a pel jugador per atacar-lo. Si el jugador fuig i s'allunya, tornarà al seu objectiu original (el Trono). Genera els arxius OpenSpec (foundations.md, spec.md i plan.md) per aquesta feature exclusiva d'intel·ligència artificial."

**Resultat de la IA:**
L'agent va entendre la funcionalitat i va generar l'esborrany inicial, però a la part d'observacions (a *spec.md*) va proposar afegir l'observació per ML-Agents d'ambdós objectius (posició del Trono i posició del Jugador) simultàniament a la llista d'observacions del sensor.

**Problema Detectat / El meu Prompt de Correcció (Refinament):**
> "Hi ha un error arquitectònic greu al document `spec.md`. Si afegim la posició d'un segon element a les observacions, estarem alterant el tamany del VectorSensor, la qual cosa m'obligarà a re-entrenar tota la xarxa neuronal des de zero i trencarà el cervell actual en Unity. Modifica l'especificació: en lloc d'observar els dos elements, l'agent només ha d'observar el vector de direcció cap a on apunta 'objectiuActual'. D'aquesta manera enganyem al cervell de ML-Agents perquè només segueixi el punt desitjat, sense importar qui és i sense haver de canviar la mida de les entrades neuronals. Actualitza el pla."

**Resultat IA:**
La IA va rectificar encertadament, mantenint el tamany del `VectorSensor` igual que l'original i simplificant molt la implementació. Aquesta decisió d'arquitectura estalvia el reentrenament complet.

---

### Iteració 2: Implementació de Lògica i Variables (opsx:apply)

**El meu Prompt:**
> "A partir del fitxer `specs/plan.md`, executa les fases 1 i 2 (opsx:apply) sobre el codi de `EnemigoAgente.cs`. Afegeix les variables del jugador, el mètode ActualizarObjetivo, i fes que el codi busqui el GameObject del jugador automàticament a l'Initialize si l'usuari no l'ha afegit a mà a l'Inspector."

**Resultat de la IA:**
L'agent va afegir correctament les variables (`transformJugador`, `radioAggro`) i la funció `ActualizarObjetivo`. Va configurar correctament el `GameObject.FindGameObjectWithTag("Player")`. No obstant això, es va oblidar de posar controls de nulls si el jugador mor.

**Problema Detectat / El meu Prompt de Correcció:**
> "Dins de `ActualizarObjetivo()`, si el jugador mor durant el joc i desapareix, el GameObject serà destruït. Això provocarà un NullReferenceException en calcular `Vector2.Distance(..., jugador.position)`. Afegeix una comprovació tipus `if (jugador == null)` i força l'objectiu actual a ser només el Trono si això passa."

**Resultat IA:**
Va aplicar els canvis pertinents, fent el codi d'IA molt més robust de cara a l'execució normal del joc.

---

### Iteració 3: Integració de l'Atac (opsx:apply)

**El meu Prompt:**
> "Finalment, implementa les Fases 3 i 4. Modifica els apartats del ML-Agents (`CollectObservations` i `OnActionReceived`) per emprar `objectiuActual`. A més, afegeix al mètode `OnCollisionStay2D` l'atac al Jugador, activant l'animació."

**Resultat de la IA:**
L'agent va reemplaçar correctament `objetivo` per `objectiuActual` en els càlculs vectors. A la col·lisió, va afegir correctament `else if (collision.gameObject.CompareTag("Player"))`, però va invocar la funció de dany de manera incorrecta, intentant cridar a `collision.gameObject.GetComponent<VidaTrono>()` dins de la secció de jugador.

**Problema Detectat / El meu Prompt de Correcció:**
> "A `OnCollisionStay2D`, quan xoca contra el Player (Jugador), estàs utilitzant el component `VidaTrono` per intentar treure-li vida. El Jugador no té aquest component. Cerca el component equivalent com `VidaJugador` i aplica la baixada de vida sobre aquest."

**Resultat IA:**
L'agent ho va canviar, resolent el problema d'identificació de components als col·lisionadors.

---

### Conclusió i Reflexió final (Anàlisi del resultat)

1. **L'agent ha seguit realment l'especificació?**
L'agent ha seguit raonablement bé els documents de Spec i Plan que havia generat en la fase de 'propose'. No obstant, en el moment de la veritat (fase *apply*) oblida detalls implícits del motor que només un humà preveu de forma natural.

2. **Quantes iteracions han estat necessàries?**
Han estat necessàries **3 iteracions clares**. En general, les iteracions s'han donat per afinar arquitectures de Unity, no pas per problemes purs de programació en C#.

3. **On falla més la IA?**
La IA falla massivament en dos punts de **context implícit**:
- **Null References en entorns vius (Unity):** Omet totalment considerar que un objecte pugui ser eliminat (mort del jugador) a mig combat.
- **Acoblament (Component Copy-Paste):** En intentar replicar el codi de dany que hi havia, ha copiat el nom sencer del component `VidaTrono` aplicant-lo malament a l'enemic. Això demostra problemes en la *coherència lògica de components externs*.

4. **Has hagut de modificar la especificació o només els prompts?**
Només vaig haver de modificar severament l'especificació inicial a la Iteració 1 per evitar destrossar la xarxa neuronal amb l'augment d'entrades. A partir d'aquí, tenir un Spec correcte ha permès que només hagi de fer petits "prompts de correcció" sense canviar els objectius finals. El desenvolupament basat en especificació ha demostrat ser molt menys frustrant i més acotat que el pur "prova i error" amb la IA.
