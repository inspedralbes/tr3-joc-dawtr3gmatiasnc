using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    [Header("Configuración Básica")]
    public Transform objetivo;
    public float velocidadSuavizado = 10f;

    [Header("Límites del Mapa")]
    public bool usarLimites = true;
    public float limiteMinX = -10f;
    public float limiteMaxX = 10f;
    public float limiteMinY = -10f;
    public float limiteMaxY = 10f;

    [Header("Poder de Desarrollador (Zoom)")]
    public Camera miCamara;
    public float zoomJuego = 8f;
    public float zoomMapa = 15f;

    void Start()
    {
        miCamara = GetComponent<Camera>();
        // El objetivo ahora se asigna automáticamente desde el script del jugador
    }

    void LateUpdate()
    {
        if (objetivo != null)
        {
            float posX = objetivo.position.x;
            float posY = objetivo.position.y;

            if (usarLimites)
            {
                posX = Mathf.Clamp(posX, limiteMinX, limiteMaxX);
                posY = Mathf.Clamp(posY, limiteMinY, limiteMaxY);
            }

            Vector3 posicionDestino = new Vector3(posX, posY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, posicionDestino, velocidadSuavizado * Time.deltaTime);
        }

        if (miCamara != null)
        {
            if (Input.GetKey(KeyCode.Z))
                miCamara.orthographicSize = Mathf.Lerp(miCamara.orthographicSize, zoomMapa, Time.deltaTime * 5f);
            else
                miCamara.orthographicSize = Mathf.Lerp(miCamara.orthographicSize, zoomJuego, Time.deltaTime * 5f);
        }
    }
}