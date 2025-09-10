using Game.Runtime.Items.Data;
using Game.Runtime.Store.Areas;
using UnityEngine;


  [CreateAssetMenu(fileName = "MachineData", menuName = "Game/Store/Shelf Data")]
public class DisplayShelfData :PurchasableAreaData
{
    public ItemType AcceptedItemType;
}
