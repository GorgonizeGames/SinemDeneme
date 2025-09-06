using UnityEngine;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.Items.Interfaces
{
    public interface IPickupable
    {
        string ItemId { get; }
        Transform Transform { get; }
        bool CanBePickedUp { get; }
        void OnPickedUp(ICarrier carrier); 
        void OnDropped(Vector3 dropPosition);
        Vector3 GetCarryOffset();
        Vector3 GetCarryRotation();
    }
}
