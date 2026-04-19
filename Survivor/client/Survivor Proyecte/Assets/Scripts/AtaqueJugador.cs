using UnityEngine;

public class AtaqueJugador : MonoBehaviour
{
    public int dano = 1;

    void OnTriggerEnter2D(Collider2D otroObjeto)
    {
        // Evitamos hacernos daño a nosotros mismos (cualquier padre/raiz)
        if (transform.root == otroObjeto.transform.root) return;

        // 1. ¿Es un enemigo normal (IA)?
        if (otroObjeto.CompareTag("Enemigo"))
        {
            VidaEnemigo vida = otroObjeto.GetComponent<VidaEnemigo>();
            if (vida != null) vida.RecibirDano(dano);
        }

        // 2. ¿Es el otro jugador (PvP)?
        if (otroObjeto.CompareTag("Player") || otroObjeto.GetComponent<VidaJugador>() != null)
        {
            VidaJugador vidaJ = otroObjeto.GetComponent<VidaJugador>();
            MovimientoCaballero mov = otroObjeto.GetComponent<MovimientoCaballero>();

            if (vidaJ != null && mov != null)
            {
                if (!mov.esMiJugador) 
                {
                    // Multijugador: He golpeado al rival visualmente en mi pantalla. Le aviso por red.
                    if (WebSocketManager.Instance != null)
                    {
                        WebSocketManager.Instance.SendNetworkMessage("ATTACK", "HIT");
                    }
                }
                else
                {
                    // Singleplayer o la IA me pega a mí
                    vidaJ.RecibirDaño(dano);
                }
            }
        }
    }
}