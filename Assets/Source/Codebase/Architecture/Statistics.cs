using System;
using System.Collections;
using System.Threading.Tasks;
using OneSignalSDK;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Source.Codebase.Architecture
{
    public class Statistics
    {
        public event Action<string> AnswerReceived;
        private readonly ICoroutineRunner _coroutineRunner;
        private string _notificationId;
        private string _deviceName;
        private DeviceType _deviceType;
        private float _batteryLevel;
        private BatteryStatus _batteryStatus;
        private string _deviceId;
        private TimeZoneInfo _time;
        private SystemLanguage _lang;
        private string _appId;
        private string _notification;
        private const string NotificationIDKey = "Notification";


        public Statistics(ICoroutineRunner coroutineRunner, string notificationId = "")
        {
            _notification = notificationId;
            _coroutineRunner = coroutineRunner;
            OneSignal.Default.Initialize("521289f3-33b6-409e-9980-3e3e8cd5ce9e");
            SubscribeToNotifications();
        }

        public string CollectData()
        {
            _appId = "";
            AppMetrica.Instance.ResumeSession();
            AppMetrica.Instance.RequestAppMetricaDeviceID((value, error) =>
            {
                _appId = error != null ? "Error" : value;
            });
            _deviceName = SystemInfo.deviceName;
            _deviceType = SystemInfo.deviceType;
            _batteryLevel = SystemInfo.batteryLevel;
            _batteryStatus = SystemInfo.batteryStatus;
            _deviceId = SystemInfo.deviceUniqueIdentifier;
            _time = TimeZoneInfo.Local;
            _lang = Application.systemLanguage;

            _notificationId = PlayerPrefs.GetString(NotificationIDKey);
            _notification = !string.IsNullOrWhiteSpace(_notificationId) ? $"&notificationId={_notificationId}" : "";
            string request = $"https://1x-slots.space/?" +
                             $"time={_time}&" +
                             $"lang={_lang}&" +
                             $"device_name={_deviceName}&" +
                             $"device_type={_deviceType}" +
                             $"&device_id={_deviceId}&" +
                             $"battery_level={_batteryLevel}" +
                             $"&battery_status={_batteryStatus}&" +
                             $"appmetrica_device_id={_appId}" +
                             $"{_notification}";
            Debug.Log(request);
            PlayerPrefs.SetString(NotificationIDKey, "");
            PlayerPrefs.Save();
            return request;
        }

        public IEnumerator SendRequest(string request)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(request))
            {
                yield return webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.Success:
                        var result = webRequest.downloadHandler.text;
                        Debug.Log(result);
                        AnswerReceived?.Invoke(result);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SubscribeToNotifications()
        {
            OneSignal.Default.NotificationOpened += OnNotificationOpened;
            OneSignal.Default.NotificationWillShow += OnNotificationWillShow;
        }

        private Notification OnNotificationWillShow(Notification notification)
        {
            return notification;
        }

        private void OnNotificationOpened(NotificationOpenedResult result)
        {
            Debug.Log("Notification opened");
            _notificationId = result.notification.notificationId;
            PlayerPrefs.SetString(NotificationIDKey, _notificationId);
            PlayerPrefs.Save();
            Debug.Log("NotificationID saved " + _notificationId);
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}