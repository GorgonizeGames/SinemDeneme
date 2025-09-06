using Game.Runtime.Items.Interfaces;
using UnityEngine;

namespace Game.Runtime.Character.Interfaces
{
    public interface ICarryingController
    {
        bool IsCarrying { get; }
        GameObject CarriedItem { get; }
        Transform CarryPoint { get; }
        bool CanPickupItem(IPickupable item);
        bool TryPickupItem(IPickupable item);
        bool TryDropItem();
        void ForceDropItem();
    }
}
