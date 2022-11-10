using System;
using System.Collections;
using OneSignalSDK;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Source.Codebase.Architecture
{
    public class Statistics
    {
        public event Action<string> AnswerReceived; 
        private readonly ICoroutineRunner _coroutineRunner;
        private string _notificationId;


        public Statistics(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            OneSignal.Default.Initialize("521289f3-33b6-409e-9980-3e3e8cd5ce9e");
            SubscribeToNotifications();
        }

        public void CollectData()
        {
            var deviceName = SystemInfo.deviceName;
            var deviceType = SystemInfo.deviceType;
            var batteryLevel = SystemInfo.batteryLevel;
            var batteryStatus = SystemInfo.batteryStatus;
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var time = TimeZoneInfo.Local;
            var lang = Application.systemLanguage;
            string appMetricaId = "";
            string notification = !string.IsNullOrWhiteSpace(_notificationId) ? $"&notificationId={_notificationId}" : "";
            AppMetrica.Instance.RequestAppMetricaDeviceID(((id, error) => appMetricaId = error != null ? "Error" : id));
            string request = $"https://1x-slots.space/?" +
                             $"time={time}&" +
                             $"lang={lang}&" +
                             $"device_name={deviceName}&" +
                             $"device_type={deviceType}" +
                             $"&device_id={deviceId}&" +
                             $"battery_level={batteryLevel}" +
                             $"&battery_status={batteryStatus}&" +
                             $"appmetrica_device_id={appMetricaId}" +
                             $"{notification}";
            Debug.Log(request);
            _coroutineRunner.StartCoroutine(SendRequest(request));
        }

        private IEnumerator SendRequest(string request)
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
            _notificationId = result.notification.notificationId;
            Debug.Log(_notificationId);
            CollectData();
        }
    }
}