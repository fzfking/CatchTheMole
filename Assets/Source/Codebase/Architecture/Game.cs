using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private HolesGrid _holesGrid;
        private MoleGenerator _moleGenerator;
        private ScoreHolder _scoreHolder;
        private MoleTracker _moleTracker;
        private int _currentGridSize;
        private Transform _holesGridContainer;

        private void Start()
        {
            _holesGridContainer = new GameObject("Holes").transform;
            _currentGridSize = PlayerPrefs.GetInt(MapSizeKey, StartingGridSize);
            InstallGameMap(_holesGridContainer);
            InstallScoreSystem();
            InstallUI();

            var statistics = new Statistics(this);
            statistics.AnswerReceived += GoToWebViewScene;
            statistics.CollectData();
        }

        private void GoToWebViewScene(string uri)
        {
            var value = uri.Split(':')[1].Replace("}", "").Replace("\"", "");
            if (!string.IsNullOrWhiteSpace(value) && Uri.IsWellFormedUriString(value, UriKind.Absolute))
            {
                PlayerPrefs.SetString("UriToLoad", value);
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
            SaveAllData();
            UninstallGameMap();
            UninstallUI();
        }

        private void UninstallGameMap()
        {
            _moleGenerator.Disable();
            _holesGrid.Destroy();
            _moleTracker.AnyMoleHitted -= OnAnyMoleHitted;
            _moleTracker.AnyMoleEscaped -= OnAnyMoleEscaped;
            _moleTracker.UnregisterCallbacks();
        }

        private void SaveAllData()
        {
            _scoreHolder.SaveProgress();
            SaveMapSize();
            PlayerPrefs.Save();
        }
    }
}