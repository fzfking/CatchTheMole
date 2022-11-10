using System.Collections;
using UnityEngine;

namespace Source.Codebase
{
    public interface ICoroutineRunner
    {
        public Coroutine StartCoroutine(IEnumerator enumerator);
        public void StopCoroutine(Coroutine coroutine);
    }
}