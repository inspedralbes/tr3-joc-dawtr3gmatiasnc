using UnityEngine;
using UnityEngine.SceneManagement;

public class VidaJugador : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 10;
    private int vidaActual;
    private bool invulnerable = false;

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    public void HacerInvulnerable(float tiempo)
    {
        StartCoroutine(Invulnerabilidad(tiempo));
    }

    System.Collections.IEnumerator Invulnerabilidad(float tiempo)
    {
        invulnerable = true;
        yield return new WaitForSeconds(tiempo);
        invulnerable = false;
    }

    public void RecibirDaño(int cantidad)
    {
        if (invulnerable) return;

        MovimientoCaballero mov = GetComponent<MovimientoCaballero>();
        bool soyYo = mov != null && mov.esMiJugador;

        vidaActual -= cantidad;
        Debug.Log("¡El jugador ha recibido daño! Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            if (GameManager.Instancia != null)
            {
                if (soyYo && WebSocketManager.Instance != null)
                {
                    // Avisamos al rival de que hemos muerto
                    WebSocketManager.Instance.SendNetworkMessage("ATTACK", "DEATH");
                }
                
                GameManager.Instancia.RegistrarMuerte(soyYo);
                vidaActual = vidaMaxima; 
            }
            else
            {
                PerderJuego(); 
            }
        }
    }

    void PerderJuego()
    {
        Debug.Log("¡EL JUGADOR HA MUERTO! GAME OVER ");
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Lobby"); 
    }
}
