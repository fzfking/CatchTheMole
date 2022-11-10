using System;
using Source.Codebase.Architecture.Interfaces;
using UnityEngine;

namespace Source.Codebase.Architecture
{
    public class ScoreHolder: IScoreViewer
    {
        public event Action<int> ScoreChanged;
        private const string KeyName = "Score";
        public int Score
        {
            get => _score;
            private set
            {
                _score = value;
                ScoreChanged?.Invoke(_score);
            }
        }
        private int _score;

        public ScoreHolder()
        {
            LoadProgress();
        }

        public int Add(int value)
        {
            Score += value;
            return Score;
        }

        public bool TrySubtract(int value)
        {
            if (Score - value >= 0)
            {
                Score -= value;
                return true;
            }

            return false;
        }

        public void SaveProgress()
        {
             PlayerPrefs.SetInt(KeyName, _score);
        }

        private void LoadProgress()
        {
            _score = PlayerPrefs.GetInt(KeyName, 0);
        }
    }
}