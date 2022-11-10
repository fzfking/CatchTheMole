using System.Collections;
using System.Linq;
using Source.Codebase.GameEntities;
using UnityEngine;

namespace Source.Codebase.Architecture
{
    public class MoleGenerator
    {
        private readonly Mole[] _moles;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly float _minWaitBetweenSpawn;
        private readonly float _maxWaitBetweenSpawn;
        private Coroutine _currentSpawnCoroutine;

        public MoleGenerator(Mole[] moles, ICoroutineRunner coroutineRunner, float minWait = 0.5f, float maxWait = 2f)
        {
            if (moles == null)
            {
                Debug.LogError("Moles is null.");
                return;
            }
            _moles = moles;
            foreach (var mole in moles)
            {
                mole.gameObject.SetActive(false);
            }
            _coroutineRunner = coroutineRunner;
            _minWaitBetweenSpawn = minWait;
            _maxWaitBetweenSpawn = maxWait;
        }

        public void Enable()
        {
            _currentSpawnCoroutine = _coroutineRunner.StartCoroutine(EnableRandomMole());
        }

        public void Disable()
        {
            if (_currentSpawnCoroutine == null)
            {
                Debug.LogError("Coroutine already stopped. But it shouldn't.");
                return;
            }
            _coroutineRunner.StopCoroutine(_currentSpawnCoroutine);
        }

        private IEnumerator EnableRandomMole()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(_minWaitBetweenSpawn, _maxWaitBetweenSpawn));
                var disabledMoles = _moles.Where(m => m.gameObject.activeSelf == false).ToArray();
                if (disabledMoles.Length == 0)
                {
                    continue;
                }
                var randomMole = disabledMoles[Random.Range(0, disabledMoles.Length)];
                randomMole.gameObject.SetActive(true);
                randomMole.Show();
            }
        }
    }
}