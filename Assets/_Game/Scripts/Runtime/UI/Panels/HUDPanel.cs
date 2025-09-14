using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using Game.Runtime.UI.Core;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.UI.Panels
{
    public class HUDPanel : BaseUIPanel, ICurrencyDisplay
    {
        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private RectTransform moneyIcon;
        [SerializeField] private Button settingsButton;

        [Header("Money Particle Settings")]
        [SerializeField] private GameObject moneyParticlePrefab;
        [SerializeField] private int particleCount = 3;
        [SerializeField] private float particleDuration = 1f;

        private Vector3 _moneyIconBaseScale;
        private bool _animatingMoney;

        protected override void OnInitialize()
        {
            layer = UILayer.HUD;
            
            if (moneyIcon != null)
                _moneyIconBaseScale = moneyIcon.localScale;
            
            settingsButton?.onClick.AddListener(OnSettingsClicked);

            // ✅ EVENT-DRIVEN: Money updates will come through events, not direct subscription
            // Economy events will be handled by EconomyUIHandler
        }

        private void OnSettingsClicked()
        {
            Debug.Log("⚙️ Settings from HUD clicked!");
            _uiSignals?.TriggerSettingsRequest();
        }

        // ✅ ICurrencyDisplay implementation - called by handlers
        public void UpdateMoney(float amount, bool animate = true)
        {
            if (moneyText != null)
            {
                moneyText.text = FormatMoney(amount);
                
                if (animate)
                {
                    PlayMoneyScaleAnimation();
                }
            }
        }

        public async Task PlayMoneyGainAnimation(Vector3 sourcePosition, float amount)
        {
            if (moneyIcon == null) return;

            var targetPos = moneyIcon.position;
            
            // Create money particles
            for (int i = 0; i < particleCount; i++)
            {
                CreateAndAnimateParticle(sourcePosition, targetPos, i * 0.1f);
            }

            // Wait for particles, then update currency
            await Task.Delay((int)(particleDuration * 1000));
            // Don't call UpdateMoney here - it will be called by EconomyUIHandler
        }

        private void PlayMoneyScaleAnimation()
        {
            if (_animatingMoney || moneyIcon == null) return;

            _animatingMoney = true;
            var targetScale = _moneyIconBaseScale * 1.2f;
            
            moneyIcon.DOScale(targetScale, 0.15f)
                     .SetLoops(2, LoopType.Yoyo)
                     .OnComplete(() => _animatingMoney = false);
        }

        private async void CreateAndAnimateParticle(Vector3 sourcePos, Vector3 targetPos, float delay)
        {
            if (moneyParticlePrefab == null) return;

            await Task.Delay((int)(delay * 1000));

            var particle = Instantiate(moneyParticlePrefab, transform.root);
            particle.transform.position = sourcePos;

            // Arc movement
            var midPoint = (sourcePos + targetPos) * 0.5f;
            midPoint.y += Random.Range(0.5f, 1.5f);

            var path = new Vector3[] { sourcePos, midPoint, targetPos };
            
            particle.transform.DOPath(path, particleDuration, PathType.CatmullRom)
                     .SetEase(Ease.InOutQuad)
                     .OnComplete(() => {
                         if (particle != null)
                             Destroy(particle);
                     });

            // Scale animation
            particle.transform.DOScale(Vector3.one * 0.8f, particleDuration * 0.3f)
                     .SetLoops(2, LoopType.Yoyo);
        }

        private string FormatMoney(float amount)
        {
            if (amount >= 1000000)
                return $"${amount / 1000000f:F1}M";
            else if (amount >= 1000)
                return $"${amount / 1000f:F1}K";
            else
                return $"${amount:F0}";
        }

        protected override void OnCleanup()
        {
            // ✅ No direct subscriptions to cleanup
        }
    }
}