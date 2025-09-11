using Game.Runtime.Items.Interfaces;
using Game.Runtime.Core.Interfaces;
using UnityEngine;

namespace Game.Runtime.Character.Interfaces
{
    public interface ICarryingController : ICarrier
    {
        bool IsCarrying { get; }
        GameObject CarriedItem { get; }
        bool CanPickupItem(IPickupable item);
        bool TryPickupItem(IPickupable item);
        bool TryDropItem();
        void ForceDropItem();
        void DropAllItems(); // Added missing method
    }
}