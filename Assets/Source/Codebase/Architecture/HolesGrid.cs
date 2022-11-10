using System.Linq;
using Source.Codebase.GameEntities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source.Codebase.Architecture
{
    public class HolesGrid
    {
        private readonly Hole _holePrefab;
        private readonly Transform _parent;
        private readonly Camera _camera;

        private Hole[,] _holes;

        public HolesGrid(Hole holePrefab, Transform parent, Camera camera)
        {
            _holePrefab = holePrefab;
            _parent = parent;
            _camera = camera;
        }

        public void GenerateHoles(int size)
        {
            _holes = new Hole[size, size];
            float holeScale = _camera.orthographicSize / size;
            float positionOffset = -holeScale * size / 2 + holeScale / 2;
            float yOffset = positionOffset;
            for (int i = 0; i < size; i++)
            {
                float xOffset = positionOffset;
                for (int j = 0; j < size; j++)
                {
                    _holes[i, j] = Object.Instantiate(_holePrefab, _parent);
                    _holes[i, j].transform.localScale = new Vector3(holeScale, holeScale);
                    _holes[i, j].transform.position = new Vector3(xOffset, yOffset);
                    xOffset += holeScale;
                }

                yOffset += holeScale;
            }
        }

        public Mole[] GetMoles()
        {
            if (_holes == null)
            {
                Debug.LogError("Attempting to get moles before holes generated.");
                return null;
            }
            return _holes.Cast<Hole>().Select(h => h.Mole).ToArray();
        }

        public void Destroy()
        {
            if (_holes != null && _holes.Length > 0)
            {
                foreach (var hole in _holes)
                {
                    if (hole != null)
                    {
                        Object.Destroy(hole.gameObject);
                    }
                }
            }
        }
    }
}