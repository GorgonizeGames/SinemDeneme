namespace Game.Runtime.Core.Interfaces
{
    public interface IEconomyService
    {
        int CurrentMoney { get; }
        bool CanAfford(int amount);
        bool TrySpend(int amount);
        void AddMoney(int amount);
        event System.Action<int> OnMoneyChanged;
    }
}