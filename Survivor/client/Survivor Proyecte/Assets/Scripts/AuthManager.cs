using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthManager : MonoBehaviour
{
    [Header("UI References")]
    public InputField usernameInput;
    public InputField passwordInput;
    public Text statusText;

    private string backendUrl = "http://localhost:3000/api/users";

    public void OnLoginClick() {
        StartCoroutine(SendAuthRequest("/login"));
    }

    public void OnRegisterClick() {
        StartCoroutine(SendAuthRequest("/register"));
    }

    IEnumerator SendAuthRequest(string endpoint)
    {
        statusText.text = "Conectando...";

        // 1. Creamos el objeto con los datos
        string json = "{\"username\":\"" + usernameInput.text + "\", \"password\":\"" + passwordInput.text + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // 2. Configuramos la petición POST
        UnityWebRequest request = new UnityWebRequest(backendUrl + endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 3. Enviamos y esperamos respuesta
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            statusText.text = "¡Éxito!: " + request.downloadHandler.text;
            // Aquí es donde cambiaríamos a la escena del juego
        } else {
            statusText.text = "Error: " + request.error;
        }
    }
}