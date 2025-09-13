using System;
using Game.Runtime.Store.Areas;

namespace Game.Runtime.Core.Data
{
    /// <summary>
    /// Purchasable area save data - clean, type-safe
    /// </summary>
    [System.Serializable]
    public class PurchasableAreaSaveData : BaseSaveDataModel
    {
        public string areaId;
        public PurchasableAreaState currentState;
        public int currentCost;
        public float purchaseProgress;
        public bool isActive;
        public long lastSaveTime; // Unix timestamp

        public PurchasableAreaSaveData() { }

        public PurchasableAreaSaveData(string id, PurchasableAreaState state, int cost, float progress)
        {
            this.id = id;
            this.areaId = id;
            this.currentState = state;
            this.currentCost = cost;
            this.purchaseProgress = progress;
            this.isActive = state == PurchasableAreaState.Active;
            this.lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }


    public enum PurchasableAreaState
    {
        Locked,
        Active
    }
}