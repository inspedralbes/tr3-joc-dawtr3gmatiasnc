using UnityEngine;

public class VidaEnemigo : MonoBehaviour
{
    public int saludMaxima = 3;
    private int saludActual;

    void Start()
    {
        // Al nacer, el enemigo tiene la salud a tope
        saludActual = saludMaxima;
    }

    // Esta función la llamará la espada cuando le golpee
    public void RecibirDano(int cantidadDano)
    {
        saludActual -= cantidadDano;
        Debug.Log("¡Ay! Me han dado. Salud restante: " + saludActual);

        if (saludActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log("¡Enemigo destruido!");
        // Destruye el objeto del mapa
        Destroy(gameObject); 
    }
}