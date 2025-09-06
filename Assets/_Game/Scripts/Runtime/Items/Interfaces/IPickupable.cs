using Game.Runtime.Character.Interfaces;
using UnityEngine;

namespace Game.Runtime.Items.Interfaces
{
    public interface IPickupable
    {

        string ItemId { get; }
        Transform Transform { get; }
        bool CanBePickedUp { get; }
        void OnPickedUp(ICarryingController carrier);
        void OnDropped(Vector3 dropPosition);
        Vector3 GetCarryOffset();
        Vector3 GetCarryRotation();
    }
}

