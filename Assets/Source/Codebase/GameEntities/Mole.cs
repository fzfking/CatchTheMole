using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Source.Codebase.GameEntities
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class Mole : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Hit HitPopup;
        [SerializeField] private float DisappearTime;
        [SerializeField] private float AppearTime;
        [SerializeField] private float DownTime;
        [SerializeField] private Vector2 HitPoint;
        public event Action Hit;
        public event Action Escaped;
        private SpriteRenderer _renderer;
        private Coroutine _currentWaiter;
        private bool _isPlayerAlreadyClicked;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _isPlayerAlreadyClicked = false;
        }

        public void Show()
        {
            _currentWaiter = StartCoroutine(Appear());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPlayerAlreadyClicked)
            {
                return;
            }

            Catch();
        }

        private void Catch()
        {
            StopCoroutine(_currentWaiter);
            _isPlayerAlreadyClicked = true;
            Hit?.Invoke();
            _currentWaiter = StartCoroutine(DisappearByClick());
        }

        private IEnumerator DisappearByTime()
        {
            float timeLeft = DisappearTime;
            var color = _renderer.color;
            while (timeLeft > 0)
            {
                var alpha = timeLeft / DisappearTime;
                color = new Color(color.r, color.g, color.b, alpha);
                _renderer.color = color;
                timeLeft -= Time.deltaTime;
                yield return null;
            }
            Escaped?.Invoke();
            gameObject.SetActive(false);
        }

        private IEnumerator DisappearByClick()
        {
            HitPopup.gameObject.SetActive(true);
            yield return HitPopup.Show(transform.position + (Vector3)HitPoint);
            _isPlayerAlreadyClicked = false;
            gameObject.SetActive(false);
        }

        private IEnumerator Appear()
        {
            float timePassed = 0f;
            var color = _renderer.color;
            while (timePassed < DisappearTime)
            {
                var alpha = timePassed / DisappearTime;
                color = new Color(color.r, color.g, color.b, alpha);
                _renderer.color = color;
                timePassed += Time.deltaTime;
                yield return null;
            }

            color = new Color(color.r, color.g, color.b, 1);

            yield return WaitForPlayerClick();
        }

        private IEnumerator WaitForPlayerClick()
        {
            float timePassed = 0f;
            while (timePassed < DownTime)
            {
                yield return null;
                timePassed += Time.deltaTime;
            }

            yield return DisappearByTime();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(HitPoint, Vector3.one * 0.1f);
        }
    }
}