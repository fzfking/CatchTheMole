using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OneSignalSDK;
using Source.Codebase.Data;
using Source.Codebase.GameEntities;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Source.Codebase.Architecture
{
    public class Game : MonoBehaviour, ICoroutineRunner
    {
        private const string MapSizeKey = "MapSize";
        private const string UriToLoad = "Uri";
        [SerializeField] private Hole HolePrefab;
        [SerializeField] private ScoreTracker ScoreTracker;
        [SerializeField] private Button MapSizeIncreaseButton;
        [SerializeField] private Button ResetMapSizeButton;
        [SerializeField] private int StartingGridSize;
        [SerializeField] private float MinWaitBetweenSpawn;
        [SerializeField] private float DefaultWaitMultiplier;
        [SerializeField] private int MapIncreaseCost;
        [SerializeField] private AppMetrica MetricaPrefab;
        private HolesGrid _holesGrid;
        private MoleGenerator _moleGenerator;
        private ScoreHolder _scoreHolder;
        private MoleTracker _moleTracker;
        private int _currentGridSize;
        private Transform _holesGridContainer;
        private Statistics _statistics;

        private void Start()
        {
            var metrica = FindObjectOfType<AppMetrica>();
            if (metrica == null)
            {
                Instantiate(MetricaPrefab);
            }
            else
            {
                AppMetrica.Instance.ResumeSession();
            }
            Input.backButtonLeavesApp = true;
            _holesGridContainer = new GameObject("Holes").transform;
            _currentGridSize = PlayerPrefs.GetInt(MapSizeKey, StartingGridSize);
            InstallGameMap(_holesGridContainer);
            InstallScoreSystem();
            InstallUI();
            _statistics = new Statistics(this);
            _statistics.AnswerReceived += GoToWebViewScene;
            var request = _statistics.CollectData();
            StartCoroutine(_statistics.SendRequest(request));
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (AppMetrica.Instance.ActivationConfig != null)
            {
                if (!string.IsNullOrWhiteSpace(AppMetrica.Instance.ActivationConfig.Value.ApiKey))
                {
                    AppMetrica.Instance.ResumeSession();
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (AppMetrica.Instance.ActivationConfig != null)
            {
                if (!string.IsNullOrWhiteSpace(AppMetrica.Instance.ActivationConfig.Value.ApiKey))
                {
                    AppMetrica.Instance.PauseSession();
                }
            }
        }

        private void GoToWebViewScene(string uri)
        {
            var link = JsonConvert.DeserializeObject<JsonExample>(uri).link;
            Debug.Log(link);
            if (!string.IsNullOrWhiteSpace(link) && Uri.IsWellFormedUriString(link, UriKind.Absolute))
            {
                PlayerPrefs.SetString(UriToLoad, link);
                PlayerPrefs.Save();
                SceneManager.LoadScene(1);
            }
        }

        private void InstallGameMap(Transform holesGridContainer)
        {
            _holesGrid = new HolesGrid(HolePrefab, holesGridContainer.transform, Camera.main);
            _holesGrid.GenerateHoles(_currentGridSize);
            var allMoles = _holesGrid.GetMoles();
            _moleGenerator = new MoleGenerator(allMoles, this, MinWaitBetweenSpawn,
                StartingGridSize * DefaultWaitMultiplier / _currentGridSize);
            _moleGenerator.Enable();
            _moleTracker = new MoleTracker(allMoles);
            _moleTracker.AnyMoleHitted += OnAnyMoleHitted;
            _moleTracker.AnyMoleEscaped += OnAnyMoleEscaped;
        }

        private void InstallScoreSystem()
        {
            _scoreHolder = new ScoreHolder();
            ScoreTracker.Link(_scoreHolder);
        }

        private void InstallUI()
        {
            MapSizeIncreaseButton.GetComponentInChildren<TextMeshProUGUI>().text =
                $"Increase map size\nCost: {MapIncreaseCost} points";
            MapSizeIncreaseButton.onClick.AddListener(OnUserBuyedMapIncrease);
            ResetMapSizeButton.onClick.AddListener(OnUserClickedResetMap);
        }


        private void UninstallUI()
        {
            if (MapSizeIncreaseButton == null || ResetMapSizeButton == null || ScoreTracker == null)
            {
                Debug.Log("UI is unloaded already.");
                return;
            }
            MapSizeIncreaseButton.onClick.RemoveListener(OnUserBuyedMapIncrease);
            ResetMapSizeButton.onClick.RemoveListener(OnUserClickedResetMap);
            ScoreTracker.Unlink();
        }

        private void OnUserBuyedMapIncrease()
        {
            if (_scoreHolder.TrySubtract(MapIncreaseCost))
            {
                _currentGridSize++;
                UninstallGameMap();
                InstallGameMap(_holesGridContainer);
            }
        }

        private void OnUserClickedResetMap()
        {
            _currentGridSize = StartingGridSize;
            UninstallGameMap();
            InstallGameMap(_holesGridContainer);
        }

        private void OnAnyMoleHitted()
        {
            _scoreHolder.Add(1);
        }

        private void OnAnyMoleEscaped()
        {
            _scoreHolder.TrySubtract(1);
        }

        private void SaveMapSize()
        {
            PlayerPrefs.SetInt(MapSizeKey, _currentGridSize);
        }

        private void OnDestroy()
        {
            if (this == null)
            {
                Debug.Log("Game is unloaded already.");
                return;
            }
            //_statistics.UnsubscribeFromNotifications();
            SaveAllData();
            UninstallGameMap();
            UninstallUI();
        }

        private void UninstallGameMap()
        {
            if (_moleGenerator == null)
            {
                Debug.Log("Mole generator is unloaded already.");
                return;
            }
            _moleGenerator.Disable();
            _holesGrid.Destroy();
            _moleTracker.AnyMoleHitted -= OnAnyMoleHitted;
            _moleTracker.AnyMoleEscaped -= OnAnyMoleEscaped;
            _moleTracker.UnregisterCallbacks();
        }

        private void SaveAllData()
        {
            if (this == null)
            {
                Debug.Log("Game is unloaded already.");
                return;
            }

            if (_scoreHolder == null)
            {
                Debug.Log("Score holder is unloaded already.");
                return;
            }
            _scoreHolder.SaveProgress();
            SaveMapSize();
            PlayerPrefs.Save();
        }
    }
}