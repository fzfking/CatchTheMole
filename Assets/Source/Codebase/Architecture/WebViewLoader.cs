using OneSignalSDK;
using UnityEngine;

namespace Source.Codebase.Architecture
{
    public class WebViewLoader : MonoBehaviour
    {
        private const string UriToLoad = "Uri";
        void Start()
        {
            Debug.Log(WebViewObject.IsWebViewAvailable());
            var webView = GetComponent<WebViewObject>();
            webView.Init();
            webView.SetVisibility(true);
            webView.SetMargins(0, 0, 0, 0);
            webView.LoadURL(PlayerPrefs.GetString(UriToLoad));

        }
    }
}
