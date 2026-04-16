using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class LoginController : MonoBehaviour
{
    // 1. Declaramos las variables de los paneles
    private VisualElement loginPanel;
    private VisualElement menuPanel;

    // Variables de los inputs
    private TextField usernameField;
    private TextField passwordField;
    private Label statusLabel;
    private Button loginButton;
    private Button registerButton;

    private string backendUrl = "http://localhost:3000/api/users";

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 2. Buscamos los PANELES por su nombre en el UXML
        loginPanel = root.Q<VisualElement>("login-panel");
        menuPanel = root.Q<VisualElement>("menu-panel");

        // 3. Buscamos los elementos interactivos
        usernameField = root.Q<TextField>("username-input");
        passwordField = root.Q<TextField>("password-input");
        statusLabel = root.Q<Label>("status-text");
        loginButton = root.Q<Button>("login-button");
        registerButton = root.Q<Button>("register-button");

        // Por seguridad, forzamos que al inicio se vea el login y no el menú
        loginPanel.style.display = DisplayStyle.Flex;
        menuPanel.style.display = DisplayStyle.None;

        // Asignamos los botones
        loginButton.clicked += () => StartCoroutine(SendAuth("/login"));
        registerButton.clicked += () => StartCoroutine(SendAuth("/register"));
        
        // (Opcional) Asignamos el botón de cerrar sesión para que haga lo contrario
        var logoutButton = root.Q<Button>("logout-button");
        if (logoutButton != null)
        {
            logoutButton.clicked += () => 
            {
                menuPanel.style.display = DisplayStyle.None;
                loginPanel.style.display = DisplayStyle.Flex;
                statusLabel.text = "Sessió tancada.";
            };
        }
    }

    IEnumerator SendAuth(string endpoint)
    {
        statusLabel.text = "Connectant amb el servidor...";
        
        string json = $"{{\"username\":\"{usernameField.value}\", \"password\":\"{passwordField.value}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(backendUrl + endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            statusLabel.text = "Sessió iniciada correctament!";
            
            loginPanel.style.display = DisplayStyle.None; // Ocultamos el Login
            menuPanel.style.display = DisplayStyle.Flex;  // Mostramos el Menú
            
        } else {
            statusLabel.text = "Error: Credencials incorrectes";
        }
    }
}