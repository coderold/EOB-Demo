using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Echoes.Game.Presentation
{
    public class GoogleAuthManager : MonoBehaviour
    {
        [Header("Google OAuth Settings")]
        [SerializeField] private string clientId = "894838189467-3ir7n8lva974c3i15l9md4lq8r54dj5s.apps.googleusercontent.com";
        
        private const string REDIRECT_URI = "http://localhost:8585/";

        private HttpListener _httpListener;
        private Action<string> _onSuccessCallback;
        private Action<string> _onErrorCallback;

        public void TriggerGoogleSignIn(Action<string> onTokenReceived, Action<string> onError)
        {
            _onSuccessCallback = onTokenReceived;
            _onErrorCallback = onError;

            StopAllCoroutines();
            ShutdownListener();

            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(REDIRECT_URI);
                _httpListener.Start();
            }
            catch (Exception ex)
            {
                _onErrorCallback?.Invoke($"Port Error: {ex.Message}");
                return;
            }

            string nonce = Guid.NewGuid().ToString("N");
            string authUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                             $"response_type=id_token" +
                             $"&client_id={UnityWebRequest.EscapeURL(clientId)}" +
                             $"&redirect_uri={UnityWebRequest.EscapeURL(REDIRECT_URI)}" +
                             $"&scope=openid%20email%20profile" +
                             $"&nonce={nonce}" +
                             $"&prompt=select_account";

            Application.OpenURL(authUrl);
            StartCoroutine(ListenForBrowserCallback());
        }

        private IEnumerator ListenForBrowserCallback()
        {
            while (_httpListener != null && _httpListener.IsListening)
            {
                IAsyncResult result = _httpListener.BeginGetContext(null, null);
                while (!result.IsCompleted) yield return null;

                HttpListenerContext context = _httpListener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string rawUrl = request.RawUrl; 
                Debug.Log($"[GoogleAuth] Intercepted Incoming Request Path: {rawUrl}");

                if (rawUrl == "/" || !rawUrl.Contains("id_token="))
                {
                    string jsHandshake = @"
                    <html>
                    <head>
                        <script>
                            window.onload = function() {
                                if (window.location.hash) {
                                    // Extract entire hash payload parameter array and pass it to our secondary route
                                    var hashParams = window.location.hash.substring(1);
                                    window.location.href = '/token?' + hashParams;
                                } else if (window.location.search) {
                                    // Fallback wrapper in case browser strips parameters into search queries
                                    window.location.href = '/token' + window.location.search;
                                } else {
                                    document.body.innerHTML = '<h2 style=""color:#ff4d4d"">Authentication failed: No token payload detected.</h2>';
                                }
                            }
                        </script>
                    </head>
                    <body style='font-family:sans-serif; text-align:center; padding-top:60px; background:#121212; color:#fff;'>
                        <h2>Connecting secure handshake back to Echoes of Bathala...</h2>
                        <p>Processing authorization token parameters, please wait.</p>
                    </body>
                    </html>";

                    byte[] buffer = Encoding.UTF8.GetBytes(jsHandshake);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }

                else if (rawUrl.StartsWith("/token"))
                {
                    string idToken = "";

                    if (rawUrl.Contains("id_token="))
                    {
                        try
                        {
                            string tokenSegment = rawUrl.Split(new[] { "id_token=" }, StringSplitOptions.None)[1];
                            idToken = tokenSegment.Split('&')[0];
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[GoogleAuth] Substring parsing tracking exception: {ex.Message}");
                        }
                    }

                    string successPage = "<html><body style='font-family:sans-serif; text-align:center; padding-top:60px; background:#121212; color:#fff;'>" +
                                        "<h2 style='color:#4caf50;'>Login Successful!</h2>" +
                                        "<p>Character Identity Verified. You can safely close this browser tab and return to the game.</p>" +
                                        "</body></html>";

                    byte[] buffer = Encoding.UTF8.GetBytes(successPage);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();

                    ShutdownListener();

                    if (!string.IsNullOrEmpty(idToken))
                    {
                        Debug.Log("[GoogleAuth] Success! Handing verified ID Token back to primary execution thread callbacks.");
                        
                        _onSuccessCallback?.Invoke(idToken);
                    }
                    else
                    {
                        _onErrorCallback?.Invoke("Token string structural processing returned empty parameters.");
                    }
                    yield break;
                }
            }
        }

        private void ShutdownListener()
        {
            if (_httpListener != null && _httpListener.IsListening)
            {
                _httpListener.Stop();
                _httpListener.Close();
            }
        }

        private void OnDestroy() => ShutdownListener();
    }
}