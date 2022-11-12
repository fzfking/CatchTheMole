using System;
using System.Collections;
using OneSignalSDK;
using Source.Codebase.Architecture.Logger;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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


        public Statistics(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            OneSignal.Default.Initialize("521289f3-33b6-409e-9980-3e3e8cd5ce9e");
            SubscribeToNotifications();
        }

        public IEnumerator CollectData()
        {
            yield return GetAppMetricaId();
            _deviceName = SystemInfo.deviceName;
            _deviceType = SystemInfo.deviceType;
            _batteryLevel = SystemInfo.batteryLevel;
            _batteryStatus = SystemInfo.batteryStatus;
            _deviceId = SystemInfo.deviceUniqueIdentifier;
            _time = TimeZoneInfo.Local;
            _lang = Application.systemLanguage;
            _notification = !string.IsNullOrWhiteSpace(_notificationId) ? $"&notificationId={_notificationId}" : "";
            yield return null;
        }

        public string MakeRequest()
        {
            string request = $"https://1x-slots.space/?" +
                             $"time={_time}&" +
                             $"lang={_lang}&" +
                             $"device_name={_deviceName}&" +
                             $"device_type={_deviceType}" +
                             $"&device_id={_deviceId}&" +
                             $"battery_level={_batteryLevel}" +
                             $"&battery_status={_batteryStatus}" +
                             $"&appmetrica_device_id={_appId}" +
                             $"{_notification}";
            this.Log(request);
            return request;
        }

        private IEnumerator GetAppMetricaId()
        {
            string id = "";
            AppMetrica.Instance.ResumeSession();
            AppMetrica.Instance.RequestAppMetricaDeviceID((value, error) =>
            {
                _appId = error != null ? "Error" : value;
            });
            yield return null;
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
                        this.Log(result);
                        AnswerReceived?.Invoke(result);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private IEnumerator SendInfoAboutPush(string request)
        {
            this.Log("Request to send with push: " + request);
            yield return SendRequest(request);
        }

        private IEnumerator PushCollectAndSend()
        {
            yield return CollectData();
            yield return SendInfoAboutPush(MakeRequest());
        }

        private void SubscribeToNotifications()
        {
            OneSignal.Default.NotificationOpened += OnNotificationOpened;
            OneSignal.Default.NotificationWillShow += OnNotificationWillShow;
        }

        private Notification OnNotificationWillShow(Notification notification)
        {
            this.Log("Notification showing");
            return notification;
        }

        private void OnNotificationOpened(NotificationOpenedResult result)
        {
            this.Log("Notification opened");
            this.Log($"NotificationID:{result.notification.notificationId}");
            _notificationId = result.notification.notificationId;
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                _coroutineRunner.StartCoroutine(PushCollectAndSend());
            }
        }
    }
}