using System;
using UnityEngine;

namespace Source.Codebase
{
    public class HolesGrid : MonoBehaviour
    {
        [SerializeField] private Hole HolePrefab;
        [SerializeField] private Vector2 LeftUpperPoint;
        private Hole[,] _holes;

        public void GenerateHoles(int size)
        {
            _holes = new Hole[size, size];
            float holeScale = Camera.main.orthographicSize / size;
            float positionOffset = -holeScale * size / 2 + holeScale / 2;
            float yOffset = positionOffset;
            for (int i = 0; i < size; i++)
            {
                float xOffset = positionOffset;
                for (int j = 0; j < size; j++)
                {
                    _holes[i, j] = Instantiate(HolePrefab, transform);
                    _holes[i, j].transform.localScale = new Vector3(holeScale, holeScale);
                    _holes[i, j].transform.position = new Vector3(xOffset, yOffset);
                    xOffset += holeScale;
                }

                yOffset += holeScale;
            }
        }

        private void Start()
        {
            GenerateHoles(7);
        }
    }
}