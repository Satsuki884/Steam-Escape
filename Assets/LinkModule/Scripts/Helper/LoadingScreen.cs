using System.Collections;
using System.Collections.Generic;
using LinkModule.Scripts.AppEntryPoint;
using LinkModule.Scripts.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LinkModule.Scripts.Helper
{
    public sealed class LoadingScreen : MonoBehaviour
    {
        public bool IsDone => _isDone;
        
        [SerializeField] private Image loadingImage;
        [SerializeField] private TextMeshProUGUI loadingStateText;
        [SerializeField] private ProgressBar progressBar;
        [SerializeField] private AnimationCurve progressCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private readonly float _duration = AppEntryPoint.AppEntryPoint.SERVICE_STARTUP_TIMEOUT;
        private bool _isDone;
        private Coroutine _progressRoutine;
        private LoadingScreenConfig _config;
        private readonly Queue<string> _stateQueue = new();
        private Coroutine _stateCoroutine;

        private void Awake()
        {
            progressBar = GetComponentInChildren<ProgressBar>();
        }

        public void Initialize(LoadingScreenConfig config)
        {
            _config = config;
            loadingImage.sprite = config.loadingSprite;
            FitImageCover();
            progressBar.Initialize(config);
        }
        
        private void FitImageCover()
        {
            var imageRect = loadingImage.rectTransform;

            if (loadingImage.sprite is null || imageRect.parent is not RectTransform parentRect)
                return;

            float imageW = loadingImage.sprite.rect.width;
            float imageH = loadingImage.sprite.rect.height;

            float parentW = parentRect.rect.width;
            float parentH = parentRect.rect.height;

            float scale = Mathf.Max(parentW / imageW, parentH / imageH);

            float resultW = imageW * scale;
            float resultH = imageH * scale;

            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.sizeDelta = new Vector2(resultW, resultH);
            imageRect.anchoredPosition = Vector2.zero;

            loadingImage.type = Image.Type.Simple;
            loadingImage.preserveAspect = true;
        }
        
        public void Show()
        {
            StartFakeLoading();
        }

        public void Hide(bool isHideInstant = true)
        {
            if (_progressRoutine != null)
                StopCoroutine(_progressRoutine);
        }
        
        private void StartFakeLoading()
        {
            _isDone = false;
            if (_progressRoutine != null)
                StopCoroutine(_progressRoutine);

            _progressRoutine = StartCoroutine(ProgressCoroutine());
        }
        
        public void ForceFinish()
        {
            if (_progressRoutine != null)
                StopCoroutine(_progressRoutine);

            progressBar.FillAmount = 1f;
            _isDone = true;
        }

        private IEnumerator ProgressCoroutine()
        {
            float timer = 0f;
            while (timer < _duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / _duration);
                float value = progressCurve.Evaluate(t);
                progressBar.FillAmount = value;
                yield return null;
            }

            progressBar.FillAmount = 1f;
            _isDone = true;
        }
        
        public void UpdateStateLoading(StartupState currentState)
        {
            if (_stateQueue.Count == 0 || _stateQueue.Peek() != currentState.ToString())
                _stateQueue.Enqueue(currentState.ToString());

            _stateCoroutine ??= StartCoroutine(ProcessQueue());
        }
        
        public void UpdateStateLoading(string currentUrl)
        {
            _stateQueue.Enqueue(currentUrl);
        }
        
        private IEnumerator ProcessQueue()
        {
            while (_stateQueue.Count > 0)
            {
                var state = _stateQueue.Dequeue();
                loadingStateText.text = state.ToString();

                yield return new WaitForSeconds(1.8f);
            }

            _stateCoroutine = null;
        }
    }
}