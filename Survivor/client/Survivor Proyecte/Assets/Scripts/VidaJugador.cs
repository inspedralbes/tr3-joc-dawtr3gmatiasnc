using UnityEngine;
using UnityEngine.SceneManagement;

public class VidaJugador : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 10;
    private int vidaActual;

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log("¡El jugador ha recibido daño! Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            PerderJuego();
        }
    }

    void PerderJuego()
    {
        Debug.Log("¡EL JUGADOR HA MUERTO! GAME OVER ");
        

        Time.timeScale = 1f; 
        
        SceneManager.LoadScene("Lobby"); 
    }
}
