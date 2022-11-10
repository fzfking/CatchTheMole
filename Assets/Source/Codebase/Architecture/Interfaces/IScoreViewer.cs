using System;

namespace Source.Codebase.Architecture.Interfaces
{
    public interface IScoreViewer
    {
        public event Action<int> ScoreChanged;
        public int Score { get; }
    }
}