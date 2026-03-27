using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class LoginController : MonoBehaviour
{
    private TextField usernameField;
    private TextField passwordField;
    private Label statusLabel;
    private Button loginButton;
    private Button registerButton;

    private string backendUrl = "http://localhost:3000/api/users";

    void OnEnable()
    {
        // 1. Obtenir referència al document UI
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 2. Buscar els elements pel seu 'name' definit al UXML
        usernameField = root.Q<TextField>("username-input");
        passwordField = root.Q<TextField>("password-input");
        statusLabel = root.Q<Label>("status-text");
        loginButton = root.Q<Button>("login-button");
        registerButton = root.Q<Button>("register-button");

        // 3. Assignar esdeveniments
        loginButton.clicked += () => StartCoroutine(SendAuth("/login"));
        registerButton.clicked += () => StartCoroutine(SendAuth("/register"));
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
            // Canvi d'escena o inici de joc aquí
        } else {
            statusLabel.text = "Error: Credencials incorrectes";
        }
    }
}