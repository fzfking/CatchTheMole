using System;
using Source.Codebase.GameEntities;

namespace Source.Codebase.Architecture
{
    public class MoleTracker
    {
        public event Action AnyMoleHitted;
        public event Action AnyMoleEscaped;
        private readonly Mole[] _moles;

        public MoleTracker(Mole[] moles)
        {
            _moles = moles;
            RegisterCallbacks();
        }

        public void UnregisterCallbacks()
        {
            foreach (var mole in _moles)
            {
                mole.Hit -= OnHit;
                mole.Escaped -= OnEscaped;
            }
        }

        private void RegisterCallbacks()
        {
            foreach (var mole in _moles)
            {
                mole.Hit += OnHit;
                mole.Escaped += OnEscaped;
            }
        }

        private void OnEscaped()
        {
            AnyMoleEscaped?.Invoke();
        }

        private void OnHit()
        {
            AnyMoleHitted?.Invoke();
        }
    }
}