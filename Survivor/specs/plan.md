# Estratègia d'Implementació

## Fase 1: Inicialització i noves variables al codi
1. Obriu `EnemigoAgente.cs`.
2. A la zona on tenim les variables principals, afegiu les noves referències públiques per al jugador: `public Transform jugador;` i `public float radioAggro = 5f;`.
3. Creeu la variable privada de control: `private Transform objectiuActual;`.
4. Al mètode existent `Initialize()`, configureu `objectiuActual = objetivo;` com a punt de partida.
5. Dins el mateix `Initialize()`, si la variable `jugador` és nul·la, feu un `GameObject.FindGameObjectWithTag("Player")` per buscar-lo automàticament.

## Fase 2: Implementació de la Lògica de Canvi d'Objectiu
1. Afegiu un nou mètode anomenat `ActualizarObjetivo()`.
2. Dins d'aquest mètode, comproveu que `jugador` no sigui nul.
3. Calculeu la distància: `float dist = Vector2.Distance(transform.position, jugador.position);`.
4. Afegiu el condicional: Si `dist <= radioAggro`, feu que l'objectiu actual sigui el jugador. Si no, torneu al trono.

## Fase 3: Modificació i engany al sistema de ML-Agents
1. Al mètode `CollectObservations`, crideu sempre a `ActualizarObjetivo()` abans de res per assegurar quin és el focus.
2. Actualitzeu el càlcul del vector director. Ha de dir `(objectiuActual.position - transform.position).normalized`. Això substituirà la menció antiga a `objetivo.position`.
3. Feu el mateix a `OnActionReceived` quan es calcula si estem a menys de 10f de distància per atorgar la recompensa positiva.

## Fase 4: Atac al jugador i Col·lisions
1. Aneu a la funció `OnCollisionStay2D`. Afegiu una nova branca del condicional per atrapar l'etiqueta `collision.gameObject.CompareTag("Player")`.
2. Re-apliqueu l'estructura que heu fet servir pel Trono: activar la variable boolean d'animació i aplicar dany cada X segons basat en `tiempoUltimoAtaque`.
3. Actualitzeu també la branca de `OnCollisionExit2D` per parar d'atacar si el jugador s'escapa corrent.
