using UnityEngine;
using Game.Runtime.Items.Data;
using Game.Runtime.Store.Areas;

namespace Game.Runtime.Store.Shelves
{
    [CreateAssetMenu(fileName = "ShelfData", menuName = "Game/Store/Shelf Data")]
    public class DisplayShelfData : PurchasableAreaData
    {
        [Header("Shelf Configuration")]
        public ItemType AcceptedItemType = ItemType.Comic;
        public int MaxDisplayItems = 12;
        public float RestockThreshold = 0.3f; // %30'un altına düştüğünde restock gerekir
        
        [Header("Customer Attraction")]
        public float CustomerAttraction = 1f; // Müşterilerin bu rafa ne kadar ilgi gösterdiği
        public float BrowsingTime = 2f; // Müşterilerin ortalama göz atma süresi
    }
}