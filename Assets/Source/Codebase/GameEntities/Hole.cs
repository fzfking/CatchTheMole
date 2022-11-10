using UnityEngine;

namespace Source.Codebase.GameEntities
{
    public class Hole: MonoBehaviour
    {
        [SerializeField] private Mole _mole;
        public Mole Mole => _mole;
    }
}