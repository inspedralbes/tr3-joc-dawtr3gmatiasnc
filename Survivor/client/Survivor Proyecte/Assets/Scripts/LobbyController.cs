using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyController : MonoBehaviour
{
    private ScrollView roomsScrollView;
    private TextField createRoomInput;
    private TextField joinRoomInput;
    
    private Button createButton;
    private Button joinButton;
    private Button refreshButton;
    private Button backButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Referencias a la UI
        roomsScrollView = root.Q<ScrollView>("rooms-scrollview");
        createRoomInput = root.Q<TextField>("create-room-input");
        joinRoomInput = root.Q<TextField>("join-room-input");

        createButton = root.Q<Button>("create-button");
        joinButton = root.Q<Button>("join-button");
        refreshButton = root.Q<Button>("refresh-button");
        backButton = root.Q<Button>("back-button");

        // Eventos de Botones
        createButton.clicked += OnCreateRoomClicked;
        joinButton.clicked += OnJoinRoomClicked;
        refreshButton.clicked += RequestRooms;
        backButton.clicked += () => SceneManager.LoadScene("LoginScene"); 

        // Suscribirse a los eventos del WebSocketManager
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnRoomListReceived += UpdateRoomsUI;
            WebSocketManager.Instance.OnRoomJoined += HandleRoomJoined;
            
            // Pedimos las salas al entrar a la pantalla
            RequestRooms();
        }
        else
        {
            Debug.LogError("No se encontró el WebSocketManager en la escena.");
        }
    }

    void OnDisable()
    {
        // Muy importante desuscribirse para evitar errores al cambiar de escena
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnRoomListReceived -= UpdateRoomsUI;
            WebSocketManager.Instance.OnRoomJoined -= HandleRoomJoined;
        }
    }

    private void RequestRooms()
    {
        roomsScrollView.Clear();
        var loadingLabel = new Label("Buscando salas en los reinos...");
        loadingLabel.style.color = new StyleColor(Color.white);
        roomsScrollView.Add(loadingLabel);

        WebSocketManager.Instance.GetRooms();
    }

    private void OnCreateRoomClicked()
    {
        string roomName = createRoomInput.value;
        if (!string.IsNullOrEmpty(roomName))
        {
            WebSocketManager.Instance.CreateRoom(roomName);
            // El servidor responderá con ROOM_CREATED, y si la lógica de tu server
            // mete automáticamente al creador en la sala, recibiremos JOINED_ROOM.
        }
    }

    private void OnJoinRoomClicked()
    {
        string roomId = joinRoomInput.value;
        if (!string.IsNullOrEmpty(roomId))
        {
            WebSocketManager.Instance.JoinRoom(roomId);
        }
    }

    // Se ejecuta cuando el servidor nos manda la lista de salas
    private void UpdateRoomsUI(List<RoomData> rooms)
    {
        roomsScrollView.Clear();

        if (rooms == null || rooms.Count == 0)
        {
            var emptyLabel = new Label("No hay tabernas abiertas. ¡Crea una!");
            emptyLabel.style.color = new StyleColor(Color.gray);
            roomsScrollView.Add(emptyLabel);
            return;
        }

        foreach (var room in rooms)
        {
            // Crear el contenedor de la sala (VisualElement)
            VisualElement roomItem = new VisualElement();
            roomItem.AddToClassList("room-item");

            // Texto con la info de la sala
            Label roomInfo = new Label($"{room.name} [{room.playersCount}/{room.maxPlayers}]");
            roomInfo.AddToClassList("room-item-text");

            // Botón para unirse
            Button joinBtn = new Button();
            joinBtn.text = "Unirse";
            joinBtn.AddToClassList("btn-medieval");
            joinBtn.AddToClassList("btn-small");
            
            // Si la sala está llena, deshabilitamos el botón
            if(room.playersCount >= room.maxPlayers)
            {
                joinBtn.SetEnabled(false);
                joinBtn.text = "Llena";
            }
            else
            {
                // Acción de unirse específica a este ID
                string currentRoomId = room.id; 
                joinBtn.clicked += () => WebSocketManager.Instance.JoinRoom(currentRoomId);
            }

            // Añadir elementos al contenedor
            roomItem.Add(roomInfo);
            roomItem.Add(joinBtn);

            // Añadir contenedor al ScrollView
            roomsScrollView.Add(roomItem);
        }
    }

    // Se ejecuta cuando el servidor nos confirma que entramos a una sala
    private void HandleRoomJoined(string roomId)
    {
        Debug.Log("¡Entramos a la sala! Cargando la escena del juego multijugador...");
        
        // Aquí cargas tu escena de juego multijugador
        SceneManager.LoadScene("JuegoMultiplayer"); 
    }
}
