using UnityEngine;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.Economy
{
    public class EconomyService : MonoBehaviour, IEconomyService
    {
        [SerializeField] private int startingMoney = 100;
        private int _currentMoney;

        public int CurrentMoney => _currentMoney;
        public event System.Action<int> OnMoneyChanged;

        void Awake()
        {
            _currentMoney = startingMoney;
        }

        public bool CanAfford(int amount)
        {
            return _currentMoney >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount)) return false;

            _currentMoney -= amount;
            OnMoneyChanged?.Invoke(_currentMoney);
            return true;
        }

        public void AddMoney(int amount)
        {
            _currentMoney += amount;
            OnMoneyChanged?.Invoke(_currentMoney);
        }
    }
}
