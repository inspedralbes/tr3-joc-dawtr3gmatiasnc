using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // Importante para cambiar de escena
using System.Collections;
using System.Text;

public class LoginController : MonoBehaviour
{
    private VisualElement loginPanel;
    private VisualElement menuPanel;

    private TextField usernameField;
    private TextField passwordField;
    private Label statusLabel;
    private Button loginButton;
    private Button registerButton;
    
    // Nuevo: Botón de Singleplayer
    private Button singlePlayerButton;

    private string backendUrl = "http://localhost:3000/api/users";

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 1. Referencias de Paneles
        loginPanel = root.Q<VisualElement>("login-panel");
        menuPanel = root.Q<VisualElement>("menu-panel");

        // 2. Referencias de Login
        usernameField = root.Q<TextField>("username-input");
        passwordField = root.Q<TextField>("password-input");
        statusLabel = root.Q<Label>("status-text");
        loginButton = root.Q<Button>("login-button");
        registerButton = root.Q<Button>("register-button");

        // 3. Referencias de Menú (Buscamos dentro de root o menuPanel)
        singlePlayerButton = root.Q<Button>("singleplayer-button");
        var logoutButton = root.Q<Button>("logout-button");

        // --- EVENTOS ---

        // Login y Registro
        loginButton.clicked += () => StartCoroutine(SendAuth("/login"));
        registerButton.clicked += () => StartCoroutine(SendAuth("/register"));

        // Botón de Singleplayer -> ¡A jugar!
        if (singlePlayerButton != null)
        {
            singlePlayerButton.clicked += () => {
                Debug.Log("Iniciando aventura en solitario...");
                SceneManager.LoadScene("Juego"); // Asegúrate de que se llame así en Build Settings
            };
        }

        // Botón Multijugador -> Ir al Lobby
        var multiplayerButton = root.Q<Button>("multiplayer-button");
        if (multiplayerButton != null)
        {
            multiplayerButton.clicked += () => {
                SceneManager.LoadScene("Lobby"); // Nombre de la nueva escena que crearemos
            };
        }

        // Botón Logout -> Volver atrás
        if (logoutButton != null)
        {
            logoutButton.clicked += () => {
                menuPanel.style.display = DisplayStyle.None;
                loginPanel.style.display = DisplayStyle.Flex;
            };
        }

        // Estado inicial
        loginPanel.style.display = DisplayStyle.Flex;
        menuPanel.style.display = DisplayStyle.None;
    }

    IEnumerator SendAuth(string endpoint)
    {
        statusLabel.text = "Connectant...";
        
        string json = $"{{\"username\":\"{usernameField.value}\", \"password\":\"{passwordField.value}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(backendUrl + endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            statusLabel.text = "¡Éxito!";
            
            // Guardamos el usuario para usarlo en el WebSocket o en las salas
            PlayerPrefs.SetString("username", usernameField.value);
            PlayerPrefs.Save();
            
            loginPanel.style.display = DisplayStyle.None;
            menuPanel.style.display = DisplayStyle.Flex;
        } else {
            statusLabel.text = "Error de conexión o datos incorrectos";
        }
    }
}