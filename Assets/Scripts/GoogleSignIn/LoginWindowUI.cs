using UnityEngine;
using UnityEngine.UI;

namespace Echoes.Game.Presentation
{
    public class LoginWindowUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button googleSignInButton;
        [SerializeField] private GameObject loadingOverlay;

        private void Awake()
        {
            if (googleSignInButton != null)
            {
                googleSignInButton.onClick.AddListener(OnGoogleSignInClicked);
            }
            
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(false);
            }
        }

        private async void OnGoogleSignInClicked()
        {
            SetLoadingState(true);


            bool isSuccess = await GoogleAuthService.Instance.AuthenticateUserAsync();

            SetLoadingState(false);

            if (isSuccess)
            {
                Debug.Log("[UI] Authentication successful! Transitioning scenes...");
                // SceneManager.LoadScene(1); // Load main game loop world map scene here
            }
            else
            {
                Debug.LogError("[UI] Authentication failed. Please try again.");
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            if (googleSignInButton != null) googleSignInButton.interactable = !isLoading;
            if (loadingOverlay != null) loadingOverlay.SetActive(isLoading);
        }
    }
}