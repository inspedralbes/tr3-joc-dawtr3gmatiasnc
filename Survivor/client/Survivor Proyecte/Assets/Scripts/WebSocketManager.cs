using System;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

[System.Serializable]
public class WsMessage
{
    public string type;
    public string playerId;
    public string roomId;
    public string roomName;
    public string message;
    public float x;
    public float y;
    public float dirX; 
    public float dirY; 
}

[System.Serializable]
public class RoomData
{
    public string id;
    public string name;
    public int playersCount;
    public int maxPlayers;
}

[System.Serializable]
public class WsRoomsList
{
    public string type;
    public List<RoomData> rooms;
}

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; private set; }

    WebSocket websocket;
    private string playerId;
    
    public string serverUrl = "ws://89.167.73.56:3000"; 
    
    private bool enemigoSpawneado = false; // NUEVO: Evitar doble spawn

    public event Action<List<RoomData>> OnRoomListReceived;
    public event Action<string> OnRoomJoined;

    [Header("Configuración de Partida")]
    public GameObject prefabCaballero; // Arrastra tu Prefab aquí en el Inspector
    public bool soyHost = false;       // Para saber si creaste la sala

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    async void Start()
    {
        playerId = PlayerPrefs.GetString("username", "Jugador_" + UnityEngine.Random.Range(1000, 9999));
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () => Debug.Log("¡Conectado al servidor WebSocket!");
        websocket.OnError += (e) => Debug.LogError("Error de WebSocket: " + e);
        websocket.OnClose += (e) => Debug.Log("Conexión cerrada!");

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleIncomingMessage(message);
        };

        await websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null) websocket.DispatchMessageQueue();
        #endif
    }

    private void HandleIncomingMessage(string json)
    {
        WsMessage data = JsonUtility.FromJson<WsMessage>(json);

        switch (data.type)
        {
            case "ROOM_CREATED":
                soyHost = true; // Yo creé la sala
                Debug.Log($"Sala creada exitosamente. Tu código es: {data.roomId}");
                JoinRoom(data.roomId);
                break;

            case "ROOMS_LIST":
                WsRoomsList roomList = JsonUtility.FromJson<WsRoomsList>(json);
                OnRoomListReceived?.Invoke(roomList.rooms);
                break;

            case "JOINED_ROOM":
                enemigoSpawneado = false; // Reseteamos al unirnos a la sala
                OnRoomJoined?.Invoke(data.roomId);
                
                // MAGIA: APAREZCO YO (Amb Corrutina per esperar a que carregui l'escena)
                StartCoroutine(EsperarYSpawnear());
                break;

            case "PLAYER_JOINED":
                Debug.Log($"El jugador {data.playerId} ha entrado a tu sala.");
                
                // MAGIA: APARECE EL ENEMIGO (Para el que creó la sala)
                StartCoroutine(EsperarYSpawnearRival());
                break;

            case "ERROR":
                Debug.LogError($"Error del servidor: {data.message}");
                break;
                
            case "MOVE":
                if (data.playerId != this.playerId)
                {
                    MovimientoCaballero enemigo = BuscarEnemigo();
                    if (enemigo != null) 
                        enemigo.ActualizarPosicionDesdeServidor(new Vector2(data.x, data.y), new Vector2(data.dirX, data.dirY));
                }
                break;
                
            case "ATTACK":
                if (data.playerId != this.playerId)
                {
                    if (data.message == "DEATH") 
                    {
                        if (GameManager.Instancia != null) GameManager.Instancia.RegistrarMuerte(false);
                    }
                    else if (data.message == "HIT")
                    {
                        // El rival dice que me ha dado un golpe en su pantalla
                        MovimientoCaballero[] jugadores = UnityEngine.Object.FindObjectsByType<MovimientoCaballero>(FindObjectsSortMode.None);
                        foreach (var j in jugadores)
                        {
                            if (j.esMiJugador)
                            {
                                VidaJugador vida = j.GetComponent<VidaJugador>();
                                if (vida != null) vida.RecibirDaño(1); // o el daño base
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Solo es la animación de ataque
                        MovimientoCaballero enemigo = BuscarEnemigo();
                        if (enemigo != null) enemigo.RealizarAtaque();
                    }
                }
                break;
        }
    }

    private MovimientoCaballero BuscarEnemigo()
    {
        // Corrección del error de "Object" ambiguo
        MovimientoCaballero[] jugadores = UnityEngine.Object.FindObjectsByType<MovimientoCaballero>(FindObjectsSortMode.None);
        foreach (var j in jugadores)
        {
            if (!j.esMiJugador) return j;
        }
        
        // MAGIA: APARECE EL ENEMIGO (Para el que se acaba de unir a la sala, lo carga en cuanto el otro se mueva)
        if (!enemigoSpawneado && prefabCaballero != null && GameManager.Instancia != null)
        {
            enemigoSpawneado = true;
            Transform spawnRival = soyHost ? GameManager.Instancia.spawnJugador2 : GameManager.Instancia.spawnJugador1;
            GameObject enemigo = Instantiate(prefabCaballero, spawnRival.position, Quaternion.identity);
            MovimientoCaballero script = enemigo.GetComponent<MovimientoCaballero>();
            script.Inicializar(false);
            script.ForzarPosicion(spawnRival.position);
            return script;
        }
        
        return null;
    }

    private System.Collections.IEnumerator EsperarYSpawnear()
    {
        // Esperem fins que la nova escena estigui carregada i el GameManager existeixi
        yield return new WaitUntil(() => GameManager.Instancia != null);

        // Si sóc l'Host, la partida encara NO comença (espero el segon jugador)
        if (soyHost) GameManager.Instancia.partidaEmpezada = false;
        else GameManager.Instancia.partidaEmpezada = true; // Si m'uneixo, la partida sí comença

        if (prefabCaballero != null)
        {
            Transform miSpawn = soyHost ? GameManager.Instancia.spawnJugador1 : GameManager.Instancia.spawnJugador2;
            GameObject yo = Instantiate(prefabCaballero, miSpawn.position, Quaternion.identity);
            yo.GetComponent<MovimientoCaballero>().Inicializar(true);
        }
    }

    private System.Collections.IEnumerator EsperarYSpawnearRival()
    {
        // Esperem fins que la nova escena estigui carregada i el GameManager existeixi
        yield return new WaitUntil(() => GameManager.Instancia != null);

        // Quan entra el rival, la partida COMENÇA per a tots dos
        GameManager.Instancia.partidaEmpezada = true;

        if (!enemigoSpawneado && prefabCaballero != null)
        {
            enemigoSpawneado = true;
            Transform spawnRival = soyHost ? GameManager.Instancia.spawnJugador2 : GameManager.Instancia.spawnJugador1;
            GameObject enemigo = Instantiate(prefabCaballero, spawnRival.position, Quaternion.identity);
            MovimientoCaballero script = enemigo.GetComponent<MovimientoCaballero>();
            script.Inicializar(false);
            script.ForzarPosicion(spawnRival.position);
        }
    }

    public async void CreateRoom(string roomName)
    {
        this.playerId = PlayerPrefs.GetString("username", this.playerId); // Actualizamos por si hizo login
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = "CREATE_ROOM", playerId = this.playerId, roomName = roomName };
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }

    public async void JoinRoom(string roomId)
    {
        this.playerId = PlayerPrefs.GetString("username", this.playerId); // Actualizamos por si hizo login
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = "JOIN_ROOM", playerId = this.playerId, roomId = roomId };
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }

    public async void GetRooms()
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = "GET_ROOMS" };
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }

    public async void SendMove(float x, float y, float dirX, float dirY)
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = "MOVE", playerId = this.playerId, x = x, y = y, dirX = dirX, dirY = dirY };
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }

    public async void SendAttack()
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = "ATTACK", playerId = this.playerId };
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }

    public async void SendVictory()
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = "GAME_WON", playerId = this.playerId };
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }

    public async void SendNetworkMessage(string tipo, string mensaje)
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = tipo, playerId = this.playerId, message = mensaje };
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }

    public async void LeaveRoomAndReset()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close(); // El server nos sacará y borrará la sala si queda vacía
        }
        
        soyHost = false;
        enemigoSpawneado = false;
        
        // Reconectamos para seguir usando el Lobby
        websocket = new WebSocket(serverUrl);
        websocket.OnOpen += () => Debug.Log("Reconectado!");
        websocket.OnMessage += (bytes) => HandleIncomingMessage(System.Text.Encoding.UTF8.GetString(bytes));
        await websocket.Connect();
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null) await websocket.Close();
    }
}