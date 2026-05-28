using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleAuthService : MonoBehaviour
{
    public static GoogleAuthService Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Keep empty for now since you are running purely in editor mock flow.")]
    [SerializeField] private string webClientId = "894838189467-3ir7n8lva974c3i15l9md4lq8r54dj5s.apps.googleusercontent.com";
    [SerializeField] private string backendApiUrl = "http://localhost:5190/api/auth/google"; 

    [Header("Testing")]
    [SerializeField] private bool useMockFlow = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<bool> AuthenticateUserAsync()
    {
        string idToken = "";

        // If you are inside the editor or have mock checked, skip external popups completely
        if (Application.isEditor || useMockFlow)
        {
            Debug.Log("[Auth] Executing Editor Mock Flow...");
            idToken = GenerateMockGoogleToken();
            // Simulate a brief network lag for presentation pacing
            await Task.Delay(1000); 
        }
        else
        {
            Debug.Log("[Auth] Running Live Google Native Standalone SDK...");
            // Real Google SDK initialization hook goes here when you finally build
            // idToken = await FetchNativeGoogleTokenAsync(webClientId);
        }

        if (string.IsNullOrEmpty(idToken))
        {
            return false;
        }

        // Dispatch verification payload to your teammate's running server API
        return await VerifyTokenWithBackendAsync(idToken);
    }

    private string GenerateMockGoogleToken()
    {
        // Simple mock JWT format layout to satisfy backend API json contracts
        return "mock_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6Ikplc3VzIiwiZWRpdG9yIjp0cnVlfQ";
    }

    private async Task<bool> VerifyTokenWithBackendAsync(string idToken)
    {
        // Construct the expected data contract payload shape
        AuthPayload payload = new AuthPayload { idToken = idToken };
        string jsonPayload = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(backendApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            // Wait until the response safely frames out from the server background task
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"[Backend] Response payload received: {jsonResponse}");
                
                // Save your custom game session details/tokens here
                // GameSessionManager.Instance.SaveSession(jsonResponse);
                return true;
            }
            else
            {
                Debug.LogError($"[Backend Error] Status Code: {request.responseCode} | Details: {request.error}");
                return false;
            }
        }
    }

    [Serializable]
    private class AuthPayload
    {
        public string idToken;
    }
}