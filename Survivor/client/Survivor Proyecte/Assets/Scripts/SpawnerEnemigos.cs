using UnityEngine;

public class SpawnerEnemigos : MonoBehaviour
{
    [SerializeField] private GameObject enemigoPrefab; 
    
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoEntreSpawns = 2f; 
    
    // 1. Añadimos un límite de seguridad para que el juego no explote
    [SerializeField] private float tiempoMinimoSpawn = 0.3f; 
    
    // 2. ¿Cuánto tiempo le restamos al reloj tras cada enemigo? (0.05f es un buen ritmo)
    [SerializeField] private float factorAceleracion = 0.05f; 

    private Transform[] puntosDeSpawn;
    private Transform trono;
    private float temporizador;

    void Start()
    {
        GameObject[] puntos = GameObject.FindGameObjectsWithTag("PuntoSpawn");
        
        puntosDeSpawn = new Transform[puntos.Length];
        for (int i = 0; i < puntos.Length; i++)
        {
            puntosDeSpawn[i] = puntos[i].transform;
        }

        trono = GameObject.FindGameObjectWithTag("Trono").transform;
        temporizador = tiempoEntreSpawns;
    }

    void Update()
    {
        temporizador -= Time.deltaTime;

        if (temporizador <= 0f)
        {
            CrearEnemigo();
            temporizador = tiempoEntreSpawns; 
        }
    }

    void CrearEnemigo()
    {
        if (puntosDeSpawn.Length == 0) return; 

        int dado = Random.Range(0, puntosDeSpawn.Length);
        Transform puntoElegido = puntosDeSpawn[dado];

        GameObject clon = Instantiate(enemigoPrefab, puntoElegido.position, Quaternion.identity);
        clon.GetComponent<EnemigoAgente>().objetivo = trono;

        // 3. ¡LA MAGIA DE LA DIFICULTAD!
        // Cada vez que sale un enemigo, hacemos que el siguiente salga un poco más rápido.
        // Solo lo bajamos si aún no hemos llegado al límite mínimo.
        if (tiempoEntreSpawns > tiempoMinimoSpawn)
        {
            tiempoEntreSpawns -= factorAceleracion;
            
            // Un pequeño seguro extra por si la resta lo baja de golpe más allá del límite
            if (tiempoEntreSpawns < tiempoMinimoSpawn) 
            {
                tiempoEntreSpawns = tiempoMinimoSpawn;
            }
        }
    }
}