using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Echoes.Game.Presentation;

public class LoginUIController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GoogleAuthManager googleAuthManager;
    [SerializeField] private AuthNetworkService authNetworkService;

    [Header("UI Elements")]
    [SerializeField] private Button googleSignInButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void OnEnable()
    {
        if (googleSignInButton != null)
            googleSignInButton.onClick.AddListener(OnGoogleSignInClicked);
    }

    private void OnDisable()
    {
        if (googleSignInButton != null)
            googleSignInButton.onClick.RemoveListener(OnGoogleSignInClicked);
    }

    private void OnGoogleSignInClicked()
    {
        SetUIInteractable(false);
        UpdateStatus("Connecting to Google...", Color.yellow);

        googleAuthManager.TriggerGoogleSignIn(
            onTokenReceived: (idToken) => {
                UpdateStatus("Verifying session with backend...", Color.yellow);
                
                authNetworkService.SendGoogleTokenToBackend(idToken, 
                    onSuccess: (userDataResponse) => {
                        UpdateStatus($"Welcome {userDataResponse.user.userName}!", Color.green);
                        
                        PlayerPrefs.SetString("Auth_AccessToken", userDataResponse.accessToken);
                        PlayerPrefs.Save();

                        SceneManager.LoadScene("StartMenu"); 
                    },
                    onFailure: (errorMessage) => {
                        UpdateStatus($"Backend Error: {errorMessage}", Color.red);
                        SetUIInteractable(true);
                    }
                );
            },
            onError: (errorDetails) => {
                UpdateStatus($"Google Login Failed: {errorDetails}", Color.red);
                SetUIInteractable(true);
            }
        );
    }

    private void UpdateStatus(string message, Color textColor)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = textColor;
        }
        Debug.Log($"[LoginUI] {message}");
    }

    private void SetUIInteractable(bool state)
    {
        if (googleSignInButton != null)
            googleSignInButton.interactable = state;
    }
}