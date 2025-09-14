using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using Game.Runtime.UI.Core;
using Game.Runtime.UI.Signals;
using Game.Runtime.Core.DI;

namespace Game.Runtime.UI.Panels
{
    public class CheatPanel : BaseUIPanel
    {
        [Header("Cheat Controls")]
        [SerializeField] private Button addMoneyButton;
        [SerializeField] private Button clearDataButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_InputField moneyInput;
        [SerializeField] private int defaultMoneyAmount = 1000;
        
        [Header("Visual Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;

        private int _moneyToAdd;

        protected override void OnInitialize()
        {
            layer = UILayer.Debug;
            _moneyToAdd = defaultMoneyAmount;
            
            addMoneyButton?.onClick.AddListener(OnAddMoneyClicked);
            clearDataButton?.onClick.AddListener(OnClearDataClicked);
            closeButton?.onClick.AddListener(OnCloseClicked);
            
            moneyInput?.onValueChanged.AddListener(OnMoneyInputChanged);
            if (moneyInput != null)
                moneyInput.text = defaultMoneyAmount.ToString();
                
            if (feedbackText != null)
                feedbackText.gameObject.SetActive(false);
        }

        private void OnAddMoneyClicked()
        {
            UnityEngine.Debug.Log($"💰 Add money cheat: ${_moneyToAdd}");
            _uiSignals?.TriggerCheatMoney(_moneyToAdd);
            _ = ShowFeedback($"+${_moneyToAdd}", Color.green);
        }

        private void OnClearDataClicked()
        {
            UnityEngine.Debug.Log("🗑️ Clear data cheat activated!");
            _uiSignals?.TriggerCheatClearData();
            _ = ShowFeedback("Data Cleared!", Color.yellow);
        }

        private void OnCloseClicked()
        {
            UnityEngine.Debug.Log("❌ Close cheat panel!");
            _uiService?.HidePanelAsync<CheatPanel>();
        }

        private void OnMoneyInputChanged(string value)
        {
            if (int.TryParse(value, out int amount))
                _moneyToAdd = Mathf.Max(0, amount);
            else
                _moneyToAdd = defaultMoneyAmount;
        }

        private async Task ShowFeedback(string message, Color color)
        {
            if (feedbackText == null) return;

            feedbackText.text = message;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);

            feedbackText.transform.localScale = Vector3.zero;
            
            var sequence = DOTween.Sequence();
            sequence.Append(feedbackText.transform.DOScale(1.1f, 0.2f))
                   .Append(feedbackText.transform.DOScale(1f, 0.1f))
                   .AppendInterval(1.5f)
                   .Append(feedbackText.DOFade(0f, 0.3f))
                   .OnComplete(() => {
                       feedbackText.gameObject.SetActive(false);
                       feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 1f);
                   });

            await sequence.AsyncWaitForCompletion();
        }
    }
}