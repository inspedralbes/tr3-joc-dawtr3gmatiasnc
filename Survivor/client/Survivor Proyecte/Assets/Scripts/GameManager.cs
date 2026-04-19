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

        // Desconnectem de la sala manualment tancant el socket (ja ho farà el script si ens sortim, però és bona pràctica)
        // I carreguem el Lobby de nou
        SceneManager.LoadScene("Lobby");
    }

    public void ReiniciarRondaLocal()
    {
        MovimientoCaballero[] jugadores = Object.FindObjectsByType<MovimientoCaballero>(FindObjectsSortMode.None);
        
        foreach (var jugador in jugadores)
        {
            if (jugador.esMiJugador) 
            {
                jugador.transform.position = spawnJugador1.position;
            }
            else
            {
                jugador.transform.position = spawnJugador2.position;
            }
        }
    }
}