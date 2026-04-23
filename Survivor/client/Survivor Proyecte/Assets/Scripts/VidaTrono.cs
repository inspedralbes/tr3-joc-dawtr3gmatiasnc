using UnityEngine;
using UnityEngine.SceneManagement;

public class VidaTrono : MonoBehaviour
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
        Debug.Log("¡El trono ha recibido daño! Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            PerderJuego();
        }
    }

    void PerderJuego()
    {
        Debug.Log("¡EL TRONO HA SIDO DESTRUIDO! GAME OVER ");
        
       
        Time.timeScale = 1f; 
        
        SceneManager.LoadScene("Lobby"); 
    }
}