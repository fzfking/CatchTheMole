using System.Collections;
using UnityEngine;

namespace Source.Codebase.GameEntities
{
    public class Hit : MonoBehaviour
    {
        [SerializeField] private float PopUpTime;
        [SerializeField] private float PopUpSpeed;

        public IEnumerator Show(Vector2 appearPosition)
        {
            transform.position = appearPosition;
            var position = transform.position;
            var scale = transform.localScale;
            scale = new Vector3(2, 2, 1);
            transform.localScale = scale;
            float timePassed = 0f;
            while (timePassed < PopUpTime)
            {
                yield return null;
                timePassed += Time.deltaTime;
                var scaleRatio = timePassed / PopUpTime;
                scale = new Vector3(2 - scaleRatio, 2 - scaleRatio, 1);
                transform.localScale = scale;
                position.y += PopUpSpeed * Time.deltaTime;
                transform.position = position;
            }

            gameObject.SetActive(false);
        }
    }
}