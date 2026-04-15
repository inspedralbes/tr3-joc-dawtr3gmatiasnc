using UnityEngine;

public class SpawnerEnemigos : MonoBehaviour
{
    // Arrastraremos el Prefab azul del enemigo aquí en el Inspector
    [SerializeField] private GameObject enemigoPrefab; 
    
    // ¿Cada cuántos segundos sale un enemigo?
    [SerializeField] private float tiempoEntreSpawns = 2f; 

    private Transform[] puntosDeSpawn;
    private Transform trono;
    private float temporizador;

    void Start()
    {
        // 1. Al darle al Play, el juego busca automáticamente TODOS los puntos invisibles
        GameObject[] puntos = GameObject.FindGameObjectsWithTag("PuntoSpawn");
        
        // Los guardamos en una lista
        puntosDeSpawn = new Transform[puntos.Length];
        for (int i = 0; i < puntos.Length; i++)
        {
            puntosDeSpawn[i] = puntos[i].transform;
        }

        // 2. Buscamos dónde está el Trono para poder decírselo a los enemigos nuevos
        trono = GameObject.FindGameObjectWithTag("Trono").transform;

        // Iniciamos el reloj
        temporizador = tiempoEntreSpawns;
    }

    void Update()
    {
        // El reloj va restando el tiempo
        temporizador -= Time.deltaTime;

        // Cuando el reloj llega a cero... ¡PUM! Sale un enemigo
        if (temporizador <= 0f)
        {
            CrearEnemigo();
            temporizador = tiempoEntreSpawns; // Volvemos a empezar el reloj
        }
    }

    void CrearEnemigo()
    {
        // Si por algún motivo olvidaste poner los puntos, esto evita que el juego explote
        if (puntosDeSpawn.Length == 0) return; 

        // 3. Tiramos unos dados para elegir de qué punto va a salir (0, 1, 2 o 3)
        int dado = Random.Range(0, puntosDeSpawn.Length);
        Transform puntoElegido = puntosDeSpawn[dado];

        // 4. Creamos al clon del enemigo en la posición del punto elegido
        GameObject clon = Instantiate(enemigoPrefab, puntoElegido.position, Quaternion.identity);

        // 5. ¡VITAL! Le enchufamos la dirección del Trono al cerebro del nuevo clon
        clon.GetComponent<EnemigoAgente>().objetivo = trono;
    }
}