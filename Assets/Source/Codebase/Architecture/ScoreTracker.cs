using Source.Codebase.Architecture.Interfaces;
using TMPro;
using UnityEngine;

namespace Source.Codebase.Architecture
{
    public class ScoreTracker : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ScoreView;
        private IScoreViewer _scoreViewer;

        public void Link(IScoreViewer scoreViewer)
        {
            _scoreViewer = scoreViewer;
            _scoreViewer.ScoreChanged += UpdateText;
            UpdateText(_scoreViewer.Score);
        }

        public void Unlink()
        {
            if (_scoreViewer == null)
            {
                Debug.LogError("Attempting to unlink null score viewer.");
                return;
            }

            _scoreViewer.ScoreChanged -= UpdateText;
        }

        private void UpdateText(int value)
        {
            ScoreView.text = value.ToString();
        }
    }
}