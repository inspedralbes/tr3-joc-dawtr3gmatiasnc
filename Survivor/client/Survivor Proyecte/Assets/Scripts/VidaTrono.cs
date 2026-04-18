using UnityEngine;
using UnityEngine.SceneManagement;

public class VidaTrono : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 10;
    private int vidaActual;

    void Start()
    {
        // Al empezar la partida, la vida está al máximo
        vidaActual = vidaMaxima;
    }

    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log("¡El trono ha recibido daño! Vida restante: " + vidaActual);

        // Si la vida llega a cero...
        if (vidaActual <= 0)
        {
            PerderJuego();
        }
    }

    void PerderJuego()
    {
        Debug.Log("¡EL TRONO HA SIDO DESTRUIDO! GAME OVER ");
        
        // Es MUY importante restaurar el tiempo normal (1f) antes de cargar la escena
        // porque si no la nueva escena cargará con el tiempo congelado.
        Time.timeScale = 1f; 
        
        // Cargamos la escena del Lobby
        SceneManager.LoadScene("Lobby"); 
    }
}