using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Configuración de Puntos")]
    public int puntosParaGanar = 3;
    
    public int puntosMios = 0;
    public int puntosRival = 0;

    [Header("Spawns")]
    public Transform spawnJugador1;
    public Transform spawnJugador2;

    [Header("Estado de la partida")]
    public bool partidaEmpezada = false;
    private string mensajePantalla = "";

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 50;
        style.alignment = TextAnchor.MiddleCenter;

        if (!partidaEmpezada && string.IsNullOrEmpty(mensajePantalla))
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "ESPERANDO RIVAL...", style);
        }
        else if (!string.IsNullOrEmpty(mensajePantalla))
        {
            style.fontSize = 70;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), mensajePantalla, style);
        }
    }

    public void RegistrarMuerte(bool soyYoElMuerto)
    {
        if (!partidaEmpezada) return; // Evita registrar muertes si el juego ya terminó y estamos esperando a salir

        if (soyYoElMuerto) puntosRival++;
        else puntosMios++;

        ComprobarGanador();
    }

    void ComprobarGanador()
    {
        if (puntosMios >= puntosParaGanar)
        {
            // T'assegures d'avisar al servidor que tu ets el guanyador per desar-ho a MongoDB!
            if (WebSocketManager.Instance != null) WebSocketManager.Instance.SendVictory();
            StartCoroutine(TerminarJuego("¡HAS GANADO!"));
        }
        else if (puntosRival >= puntosParaGanar)
        {
            StartCoroutine(TerminarJuego("¡HAS PERDIDO!"));
        }
        else
        {
            ReiniciarRondaLocal();
        }
    }

    System.Collections.IEnumerator TerminarJuego(string mensaje)
    {
        mensajePantalla = mensaje;
        partidaEmpezada = false; // Bloqueja moviments

        // Esperem 3 segons per veure el missatge
        yield return new WaitForSeconds(3f);

        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.LeaveRoomAndReset();
        }

        // Carreguem el Lobby de nou
        SceneManager.LoadScene("Lobby");
    }

    public void ReiniciarRondaLocal()
    {
        MovimientoCaballero[] jugadores = Object.FindObjectsByType<MovimientoCaballero>(FindObjectsSortMode.None);
        
        bool soyHost = false;
        if (WebSocketManager.Instance != null) soyHost = WebSocketManager.Instance.soyHost;

        foreach (var jugador in jugadores)
        {
            MovimientoCaballero mov = jugador.GetComponent<MovimientoCaballero>();
            if (mov == null) continue;

            if (jugador.esMiJugador) 
            {
                mov.ForzarPosicion(soyHost ? spawnJugador1.position : spawnJugador2.position);
                VidaJugador vida = mov.GetComponent<VidaJugador>();
                if (vida != null) vida.HacerInvulnerable(1.5f); // 1.5s de inmunidad al respawnear
            }
            else
            {
                mov.ForzarPosicion(soyHost ? spawnJugador2.position : spawnJugador1.position);
            }
        }
    }
}