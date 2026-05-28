using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthNetworkService : MonoBehaviour
{
    private const string BASE_URL = "http://localhost:5190/api";

    public void SendGoogleTokenToBackend(string idToken, Action<GoogleLoginResponse> onSuccess, Action<string> onFailure)
    {
        StartCoroutine(PostGoogleAuthCoroutine(idToken, onSuccess, onFailure));
    }

    private IEnumerator PostGoogleAuthCoroutine(string idToken, Action<GoogleLoginResponse> onSuccess, Action<string> onFailure)
    {
        string url = $"{BASE_URL}/auth/google";

        // Create request payload
        GoogleLoginRequest requestBody = new GoogleLoginRequest { idToken = idToken };
        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    GoogleLoginResponse responseData = JsonUtility.FromJson<GoogleLoginResponse>(jsonResponse);
                    
                    onSuccess?.Invoke(responseData);
                }
                catch (Exception ex)
                {
                    onFailure?.Invoke($"Failed to parse backend response: {ex.Message}");
                }
            }
            else
            {
                onFailure?.Invoke($"API Error: {webRequest.error} | Response: {webRequest.downloadHandler.text}");
            }
        }
    }
}