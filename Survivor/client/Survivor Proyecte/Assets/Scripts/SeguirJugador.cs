using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    [Header("Configuración Básica")]
    public Transform objetivo;
    public float velocidadSuavizado = 10f;

    [Header("Límites del Mapa (¡NUEVO!)")]
    [Tooltip("Activa esto para que la cámara no salga del mapa")]
    public bool usarLimites = true;
    public float limiteMinX = -10f; // Tope izquierdo
    public float limiteMaxX = 10f;  // Tope derecho
    public float limiteMinY = -10f; // Tope inferior
    public float limiteMaxY = 10f;  // Tope superior

    [Header("Poder de Desarrollador (Zoom)")]
    public Camera miCamara;
    public float zoomJuego = 8f;
    public float zoomMapa = 15f;

    void Start()
    {
        miCamara = GetComponent<Camera>();
        if (objetivo == null)
        {
            GameObject jugador = GameObject.Find("Soldado1");
            if (jugador != null) objetivo = jugador.transform;
        }
    }

    void LateUpdate()
    {
        if (objetivo != null)
        {
            // 1. Capturamos la posición a la que quiere ir la cámara
            float posX = objetivo.position.x;
            float posY = objetivo.position.y;

            // 2. APLICAMOS LA MAGIA (Los Límites)
            if (usarLimites)
            {
                // Mathf.Clamp obliga a la variable a no pasarse de un mínimo o un máximo
                posX = Mathf.Clamp(posX, limiteMinX, limiteMaxX);
                posY = Mathf.Clamp(posY, limiteMinY, limiteMaxY);
            }

            // 3. Movemos la cámara a esa posición limitada
            Vector3 posicionDestino = new Vector3(posX, posY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, posicionDestino, velocidadSuavizado * Time.deltaTime);
        }

        // Sistema de Zoom
        if (miCamara != null)
        {
            if (Input.GetKey(KeyCode.Z))
                miCamara.orthographicSize = Mathf.Lerp(miCamara.orthographicSize, zoomMapa, Time.deltaTime * 5f);
            else
                miCamara.orthographicSize = Mathf.Lerp(miCamara.orthographicSize, zoomJuego, Time.deltaTime * 5f);
        }
    }
}