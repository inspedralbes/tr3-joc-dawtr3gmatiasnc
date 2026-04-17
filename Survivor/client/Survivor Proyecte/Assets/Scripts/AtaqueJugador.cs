using UnityEngine;

public class AtaqueJugador : MonoBehaviour
{
    public int dano = 1;

    // Esta función de Unity se activa SOLA cuando este Trigger toca a otro colisionador
    void OnTriggerEnter2D(Collider2D otroObjeto)
    {
        // 1. ¿Es un enemigo?
        if (otroObjeto.CompareTag("Enemigo"))
        {
            // 2. Buscamos el script de Vida en ese enemigo
            VidaEnemigo vida = otroObjeto.GetComponent<VidaEnemigo>();

            // 3. Si lo tiene, le hacemos daño
            if (vida != null)
            {
                vida.RecibirDano(dano);
            }
        }
    }
}