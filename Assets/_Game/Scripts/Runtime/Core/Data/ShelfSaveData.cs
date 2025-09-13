using System;

namespace Game.Runtime.Core.Data
{
    /// <summary>
    /// Shelf-specific save data
    /// </summary>
    [System.Serializable]
    public class ShelfSaveData : BaseSaveDataModel
    {
        public string shelfId;
        public int itemCount;
        public int totalItemsStocked;
        public int totalItemsSold;
        public float stockPercentage;
        public long lastSaveTime;

        public ShelfSaveData() { }

        public ShelfSaveData(string id, int itemCount, int stocked, int sold)
        {
            this.id = id;
            this.shelfId = id;
            this.itemCount = itemCount;
            this.totalItemsStocked = stocked;
            this.totalItemsSold = sold;
            this.stockPercentage = itemCount > 0 ? (float)itemCount / 12f : 0f; // Assuming 12 max items
            this.lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}