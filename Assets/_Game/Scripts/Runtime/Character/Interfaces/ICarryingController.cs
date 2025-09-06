using Game.Runtime.Items.Interfaces;
using Game.Runtime.Core.Interfaces;
using UnityEngine;

namespace Game.Runtime.Character.Interfaces
{
    public interface ICarryingController : ICarrier // ICarrier'dan inherit
    {
        bool IsCarrying { get; }
        GameObject CarriedItem { get; }
        bool CanPickupItem(IPickupable item);
        bool TryPickupItem(IPickupable item);
        bool TryDropItem();
        void ForceDropItem();
    }
}
