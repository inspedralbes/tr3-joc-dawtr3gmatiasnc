using System;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket; // Requiere importar el paquete NativeWebSocket

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
    
    // Aquí puedes cambiar a la IP de tu servidor si no juegas en local
    public string serverUrl = "ws://localhost:8080"; 

    public event Action<List<RoomData>> OnRoomListReceived;
    public event Action<string> OnRoomJoined;

    private void Awake()
    {
        // Patrón Singleton para acceder desde cualquier script
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    async void Start()
    {
        // Recuperamos el nombre de usuario que guardamos en el Login
        playerId = PlayerPrefs.GetString("username", "Jugador_" + UnityEngine.Random.Range(1000, 9999));

        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("¡Conectado al servidor WebSocket!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("Error de WebSocket: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Conexión cerrada!");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleIncomingMessage(message);
        };

        // Esperamos a que se conecte
        await websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
        #endif
    }

    private void HandleIncomingMessage(string json)
    {
        // Deserializamos el tipo de mensaje para saber qué hacer
        WsMessage data = JsonUtility.FromJson<WsMessage>(json);

        switch (data.type)
        {
            case "ROOM_CREATED":
                Debug.Log($"Sala creada exitosamente. Tu código es: {data.roomId}");
                // AGREGAR ESTA LÍNEA: Unity pide unirse a la sala automáticamente tras crearla
                JoinRoom(data.roomId);
                break;

            case "ROOMS_LIST":
                WsRoomsList roomList = JsonUtility.FromJson<WsRoomsList>(json);
                // Avisamos a la UI de que hay nuevas salas
                OnRoomListReceived?.Invoke(roomList.rooms);
                break;

            case "JOINED_ROOM":
                // Avisamos a la UI de que entramos a la sala
                OnRoomJoined?.Invoke(data.roomId);
                break;

            case "PLAYER_JOINED":
                Debug.Log($"El jugador {data.playerId} ha entrado a tu sala.");
                break;

            case "PLAYER_LEFT":
                Debug.Log($"El jugador {data.playerId} se ha ido de la sala.");
                break;

            case "ERROR":
                Debug.LogError($"Error del servidor: {data.message}");
                break;
                
            case "MOVE":
                // Aquí llega el movimiento de otro jugador
                // Debug.Log($"El jugador movió a X:{data.x} Y:{data.y}");
                break;
        }
    }

    // --- MÉTODOS PÚBLICOS PARA LLAMAR DESDE OTROS SCRIPTS (BOTONES DE LA UI) ---

    public async void CreateRoom(string roomName)
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage
            {
                type = "CREATE_ROOM",
                playerId = this.playerId,
                roomName = roomName
            };
            string json = JsonUtility.ToJson(msg);
            await websocket.SendText(json);
        }
    }

    public async void JoinRoom(string roomId)
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage
            {
                type = "JOIN_ROOM",
                playerId = this.playerId,
                roomId = roomId
            };
            string json = JsonUtility.ToJson(msg);
            await websocket.SendText(json);
        }
    }

    public async void GetRooms()
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage { type = "GET_ROOMS" };
            string json = JsonUtility.ToJson(msg);
            await websocket.SendText(json);
        }
    }

    public async void SendMove(float x, float y)
    {
        if (websocket.State == WebSocketState.Open)
        {
            WsMessage msg = new WsMessage
            {
                type = "MOVE",
                x = x,
                y = y
            };
            string json = JsonUtility.ToJson(msg);
            await websocket.SendText(json);
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
