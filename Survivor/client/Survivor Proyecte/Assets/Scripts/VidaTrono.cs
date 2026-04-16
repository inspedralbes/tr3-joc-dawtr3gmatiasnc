using UnityEngine;

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
        
        // Esto congela el tiempo en Unity al instante, parando el juego.
        Time.timeScale = 0f; 
        
        // Más adelante, aquí podemos hacer que aparezca un menú de "Has Perdido"
    }
}