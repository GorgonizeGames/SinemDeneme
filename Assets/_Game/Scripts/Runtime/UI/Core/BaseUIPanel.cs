using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using Game.Runtime.Core.DI;
using Game.Runtime.UI.Signals;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseUIPanel : MonoBehaviour, IUIPanel
    {
        [Header("Panel Settings")]
        [SerializeField] protected string panelId;
        [SerializeField] protected UILayer layer = UILayer.Game;
        [SerializeField] protected bool hideOnAwake = true;

        // ✅ SADECE UI sistemine ihtiyaç olan dependency'ler
        [Inject] protected IUIService _uiService;
        [Inject] protected IUISignals _uiSignals;

        // ❌ REMOVED: IGameManager, IEconomyService - Panel'lar bunları direkt bilmemeli

        protected CanvasGroup _canvasGroup;
        protected bool _isInitialized = false;
        private Tween _currentTween;

        public string PanelId => panelId;
        public bool IsVisible => _canvasGroup?.alpha > 0.01f && gameObject.activeInHierarchy;
        public UILayer Layer => layer;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (string.IsNullOrEmpty(panelId))
                panelId = GetType().Name;

            if (hideOnAwake)
                SetVisibility(false, false);
        }

        protected virtual void Start()
        {
            this.InjectDependencies();
            _uiService?.RegisterPanel(this);
            Initialize();
        }

        public virtual void Initialize()
        {
            if (_isInitialized) return;
            
            OnInitialize();
            _isInitialized = true;
        }

        public virtual async Task ShowAsync(UITransition transition = UITransition.Fade)
        {
            if (!_isInitialized) Initialize();

            await AnimateShow(transition);
            OnShow();
        }

        public virtual async Task HideAsync(UITransition transition = UITransition.Fade)
        {
            OnHide();
            await AnimateHide(transition);
        }

        // Animation methods remain the same...
        protected virtual async Task AnimateShow(UITransition transition)
        {
            if (_canvasGroup == null) return;

            KillCurrentTween();
            SetVisibility(true, false);

            switch (transition)
            {
                case UITransition.None:
                    _canvasGroup.alpha = 1f;
                    break;
                    
                case UITransition.Fade:
                    _canvasGroup.alpha = 0f;
                    _currentTween = _canvasGroup.DOFade(1f, 0.3f);
                    await _currentTween.AsyncWaitForCompletion();
                    break;
                    
                case UITransition.Scale:
                    _canvasGroup.alpha = 1f;
                    transform.localScale = Vector3.zero;
                    _currentTween = transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    await _currentTween.AsyncWaitForCompletion();
                    break;
                    
                case UITransition.FadeScale:
                    _canvasGroup.alpha = 0f;
                    transform.localScale = Vector3.zero;
                    var sequence = DOTween.Sequence();
                    sequence.Append(_canvasGroup.DOFade(1f, 0.3f));
                    sequence.Join(transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
                    _currentTween = sequence;
                    await _currentTween.AsyncWaitForCompletion();
                    break;
            }
        }

        protected virtual async Task AnimateHide(UITransition transition)
        {
            if (_canvasGroup == null) return;

            KillCurrentTween();

            switch (transition)
            {
                case UITransition.None:
                    SetVisibility(false, false);
                    break;
                    
                case UITransition.Fade:
                    _currentTween = _canvasGroup.DOFade(0f, 0.2f);
                    await _currentTween.AsyncWaitForCompletion();
                    SetVisibility(false, false);
                    break;
                    
                case UITransition.Scale:
                    _currentTween = transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
                    await _currentTween.AsyncWaitForCompletion();
                    SetVisibility(false, true);
                    break;
                    
                case UITransition.FadeScale:
                    var sequence = DOTween.Sequence();
                    sequence.Append(_canvasGroup.DOFade(0f, 0.2f));
                    sequence.Join(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
                    _currentTween = sequence;
                    await _currentTween.AsyncWaitForCompletion();
                    SetVisibility(false, true);
                    break;
            }
        }

        protected void SetVisibility(bool visible, bool resetScale)
        {
            if (_canvasGroup == null) return;

            gameObject.SetActive(visible);
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;

            if (resetScale)
                transform.localScale = Vector3.one;
        }

        private void KillCurrentTween()
        {
            if (_currentTween?.IsActive() == true)
            {
                _currentTween.Kill();
                _currentTween = null;
            }
        }

        public virtual void Cleanup()
        {
            KillCurrentTween();
            OnCleanup();
        }

        protected abstract void OnInitialize();
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnCleanup() { }

        protected virtual void OnDestroy() => Cleanup();
    }
}