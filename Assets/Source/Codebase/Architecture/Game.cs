using System;
using System.Collections;
using Newtonsoft.Json;
using Source.Codebase.Data;
using Source.Codebase.GameEntities;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Source.Codebase.Architecture.Logger.Logger;

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
        private static Statistics _statistics;

        [RuntimeInitializeOnLoadMethod]
        private static void ClearLoggerOnStartUp()
        {
            Clear();
        }

        [RuntimeInitializeOnLoadMethod]
        private static void SetUpPersistentApplicationSettings()
        {
            Input.backButtonLeavesApp = true;
        }

        private IEnumerator Start()
        {
            yield return EnableMetrics();
            _holesGridContainer = new GameObject("Holes").transform;
            _currentGridSize = PlayerPrefs.GetInt(MapSizeKey, StartingGridSize);
            InstallGameMap(_holesGridContainer);
            InstallScoreSystem();
            InstallUI();
            var request = _statistics.CollectData();
            yield return null;
            yield return _statistics.CollectData();
            yield return _statistics.SendRequest(_statistics.MakeRequest());
        }

        private IEnumerator EnableMetrics()
        {
            var metrica = FindObjectOfType<AppMetrica>();
            if (metrica == null)
            {
                this.Log("Metrica created");
                Instantiate(MetricaPrefab);
            }

            yield return null;

            if (_statistics == null)
            {
                this.Log("Statistic created");
                _statistics = new Statistics(new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>());
                _statistics.AnswerReceived += GoToWebViewScene;
            }
            
            yield return null;
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
            _statistics.AnswerReceived -= GoToWebViewScene;
            var link = JsonConvert.DeserializeObject<JsonExample>(uri).link;
            this.Log(link);
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
                this.Log("UI is unloaded already.");
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
                this.Log("Game is unloaded already.");
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
                this.Log("Mole generator is unloaded already.");
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
                this.Log("Game is unloaded already.");
                return;
            }

            if (_scoreHolder == null)
            {
                this.Log("Score holder is unloaded already.");
                return;
            }
            _scoreHolder.SaveProgress();
            SaveMapSize();
            PlayerPrefs.Save();
        }
    }
}