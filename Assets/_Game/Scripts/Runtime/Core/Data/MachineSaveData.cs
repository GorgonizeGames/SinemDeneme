using System;

namespace Game.Runtime.Core.Data
{    /// <summary>
     /// Machine-specific save data
     /// </summary>
    [System.Serializable]
    public class MachineSaveData : BaseSaveDataModel
    {
        public string machineId;
        public int itemCount;
        public int totalItemsProduced;
        public bool isProducing;
        public float lastProductionTime;
        public int upgradeLevel;
        public long lastSaveTime;

        public MachineSaveData() { }

        public MachineSaveData(string id, int itemCount, int totalProduced)
        {
            this.id = id;
            this.machineId = id;
            this.itemCount = itemCount;
            this.totalItemsProduced = totalProduced;
            this.lastProductionTime = UnityEngine.Time.time;
            this.lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
