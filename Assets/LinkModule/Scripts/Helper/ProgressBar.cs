using LinkModule.Scripts.Config;
using UnityEngine;
using UnityEngine.UI;

namespace LinkModule.Scripts.Helper
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    public class ProgressBar : MonoBehaviour
    {
        private const string SHADER_NAME = "Unlit/AdvancedProgressBar";

        private Image _image;
        private Material _material;

        // Shader property IDs
        private int _mainTexPropertyID;
        private int _mainColorPropertyID;
        private int _startColorPropertyID;
        private int _endColorPropertyID;
        private int _backColorPropertyID;
        private int _gradientPropertyID;
        private int _roundnessSizePropertyID;
        private int _borderSizePropertyID;
        private int _fillAmountPropertyID;
        private int _sizePropertyID;
        private int _glowColorPropertyID;
        private int _glowSizePropertyID;
        private int _glowPowerPropertyID;
        private int _trailSpeedPropertyID;
        private int _trailStrengthPropertyID;
        private int _trailFrequencyPropertyID;
        private int _trailWidthPropertyID;
        private int _trailCountPropertyID;
        private int _trailFadeStartPropertyID;

        // Configurable properties
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private Color endColor = Color.white;
        [SerializeField] private Color backColor = Color.black;
        [SerializeField] [Range(0f, 1f)] private float gradient;
        [SerializeField] [Range(0f, 1f)] private float roundness = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float borderSize = 0.15f;
        [SerializeField] [Range(0f, 1f)] private float fillAmount = 1f;
        [SerializeField] private Color glowColor = Color.white;
        [SerializeField] [Range(0f, 0.5f)] private float glowSize = 0.05f;
        [SerializeField] [Range(1f, 10f)] private float glowPower = 4f;
        [SerializeField] [Range(0.1f, 5f)] private float trailSpeed = 1f;
        [SerializeField] [Range(0f, 1f)] private float trailStrength = 0.3f;
        [SerializeField] [Range(1f, 10f)] private float trailFrequency = 5f;
        [SerializeField] [Range(0.01f, 0.5f)] private float trailWidth = 0.1f;
        [SerializeField] [Range(1f, 6f)] private float trailCount = 3f;
        [SerializeField] [Range(0.7f, 1f)] private float trailFadeStart = 0.9f;

        public float FillAmount
        {
            get => fillAmount;
            set => fillAmount = value;
        }

        private void Awake()
        {
            _mainTexPropertyID = Shader.PropertyToID("_MainTex");
            _mainColorPropertyID = Shader.PropertyToID("_MainColor");
            _startColorPropertyID = Shader.PropertyToID("_StartColor");
            _endColorPropertyID = Shader.PropertyToID("_EndColor");
            _backColorPropertyID = Shader.PropertyToID("_BackColor");
            _gradientPropertyID = Shader.PropertyToID("_Gradient");
            _roundnessSizePropertyID = Shader.PropertyToID("_Roundness");
            _borderSizePropertyID = Shader.PropertyToID("_BorderSize");
            _fillAmountPropertyID = Shader.PropertyToID("_FillAmount");
            _sizePropertyID = Shader.PropertyToID("_Size");
            _glowColorPropertyID = Shader.PropertyToID("_GlowColor");
            _glowSizePropertyID = Shader.PropertyToID("_GlowSize");
            _glowPowerPropertyID = Shader.PropertyToID("_GlowPower");
            _trailSpeedPropertyID = Shader.PropertyToID("_TrailSpeed");
            _trailStrengthPropertyID = Shader.PropertyToID("_TrailStrength");
            _trailFrequencyPropertyID = Shader.PropertyToID("_TrailFrequency");
            _trailWidthPropertyID = Shader.PropertyToID("_TrailWidth");
            _trailCountPropertyID = Shader.PropertyToID("_TrailCount");
            _trailFadeStartPropertyID = Shader.PropertyToID("_TrailFadeStart");

            _image = GetComponent<Image>();
            _image.material = _material = new Material(Shader.Find(SHADER_NAME));

            UpdateView();
        }

        public void Initialize(LoadingScreenConfig config)
        {
            startColor = config.startProgressBarColor;
            endColor = config.endProgressBarColor;
            backColor = config.backgroundProgressBarColor;
            glowColor = config.glowProgressBarColor;
        }

        private void Update()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            if (_image == null || _material == null) 
                return;

            // Assign texture if available
            _material.SetTexture(_mainTexPropertyID, _image.sprite != null ? _image.sprite.texture : null);

            // Assign shader properties
            _material.SetColor(_mainColorPropertyID, _image.color);
            _material.SetColor(_startColorPropertyID, startColor);
            _material.SetColor(_endColorPropertyID, endColor);
            _material.SetColor(_backColorPropertyID, backColor);
            _material.SetFloat(_gradientPropertyID, gradient);
            _material.SetFloat(_roundnessSizePropertyID, roundness);
            _material.SetFloat(_borderSizePropertyID, borderSize);
            _material.SetFloat(_fillAmountPropertyID, fillAmount);
            _material.SetColor(_glowColorPropertyID, glowColor);
            _material.SetFloat(_glowSizePropertyID, glowSize);
            _material.SetFloat(_glowPowerPropertyID, glowPower);
            _material.SetFloat(_trailSpeedPropertyID, trailSpeed);
            _material.SetFloat(_trailStrengthPropertyID, trailStrength);
            _material.SetFloat(_trailFrequencyPropertyID, trailFrequency);
            _material.SetFloat(_trailWidthPropertyID, trailWidth);
            _material.SetFloat(_trailCountPropertyID, trailCount);
            _material.SetFloat(_trailFadeStartPropertyID, trailFadeStart);

            var scale = transform.lossyScale;
            var rect = _image.rectTransform.rect;
            _material.SetVector(_sizePropertyID, new Vector4(scale.x * rect.width, scale.y * rect.height, 0, 0));
        }

        private void OnDestroy()
        {
            if (_material == null) 
                return;
            
            if (Application.isPlaying)
                Destroy(_material);
            else
                DestroyImmediate(_material);

            _material = null;
        }
    }
}