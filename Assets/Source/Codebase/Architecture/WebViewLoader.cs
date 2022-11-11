using System;
using OneSignalSDK;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Source.Codebase.Architecture
{
    public class WebViewLoader : MonoBehaviour
    {
        private const string UriToLoad = "Uri";
        void Start()
        {
            Debug.Log(WebViewObject.IsWebViewAvailable());
            if (WebViewObject.IsWebViewAvailable())
            {
                var value = PlayerPrefs.GetString(UriToLoad);
                var webView = GetComponent<WebViewObject>();
                webView.Init();
                webView.SetVisibility(true);
                webView.SetMargins(0, 0, 0, 0);
                webView.LoadURL(PlayerPrefs.GetString(UriToLoad));
            }
        }

        [ContextMenu("Load scene")]
        private void LoadFirstScene()
        {
            SceneManager.LoadScene(0);
        }
    }
}
